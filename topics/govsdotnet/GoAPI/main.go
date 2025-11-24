package main

import (
	"database/sql"
	"encoding/json"
	"fmt"
	"log"
	"net/http"
	"time"

	"github.com/gin-gonic/gin"
	_ "github.com/lib/pq"
)

// ProductDto represents the product structure
type ProductDto struct {
	Id     int    `json:"id"`
	Name   string `json:"name"`
	Number int    `json:"number"`
}

// ProductRepository wraps database access
type ProductRepository struct {
	db *sql.DB
}

func NewProductRepository(connStr string) (*ProductRepository, error) {
	db, err := sql.Open("postgres", connStr)
	if err != nil {
		return nil, err
	}
	// Optional: Ping DB
	if err := db.Ping(); err != nil {
		return nil, err
	}
	return &ProductRepository{db: db}, nil
}

func (r *ProductRepository) GetProduct(id int) (*ProductDto, error) {
	var p ProductDto
	err := r.db.QueryRow("SELECT id, name, number FROM products WHERE id=$1", id).Scan(&p.Id, &p.Name, &p.Number)
	if err != nil {
		return nil, err
	}
	return &p, nil
}

func (r *ProductRepository) UpdateProductNumber(id int, number int) (int64, error) {
	res, err := r.db.Exec("UPDATE products SET number=$1 WHERE id=$2", number, id)
	if err != nil {
		return 0, err
	}
	return res.RowsAffected()
}

func main() {
	// PostgreSQL connection string
	connStr := "host=postgres-db port=5432 user=simha password=Postgres2019! dbname=weather sslmode=disable"
	repo, err := NewProductRepository(connStr)
	if err != nil {
		log.Fatalf("Failed to connect to DB: %v", err)
	}

	router := gin.Default()

	router.GET("/process/:id", func(c *gin.Context) {
		idParam := c.Param("id")
		var id int
		if _, err := fmt.Sscanf(idParam, "%d", &id); err != nil {
			c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid id"})
			return
		}

		// 1 - Get local product
		localProduct, err := repo.GetProduct(id)
		if err != nil {
			c.JSON(http.StatusNotFound, gin.H{"error": fmt.Sprintf("Local product with id %d not found", id)})
			return
		}

		// 2 - Call remote Product API
		client := &http.Client{Timeout: 5 * time.Second}
		resp, err := client.Get(fmt.Sprintf("http://product-api:3000/product/%d", id))
		if err != nil || resp.StatusCode != http.StatusOK {
			c.JSON(http.StatusInternalServerError, gin.H{"error": "Remote Product API request failed"})
			return
		}
		defer resp.Body.Close()

		var remoteProduct ProductDto
		if err := json.NewDecoder(resp.Body).Decode(&remoteProduct); err != nil {
			c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to parse remote Product API response"})
			return
		}

		// 3 - Increment product number 10k times
		for i := 0; i < 10_000; i++ {
			remoteProduct.Number++
		}

		// 4 - Save updated number to local DB
		rowsAffected, err := repo.UpdateProductNumber(id, remoteProduct.Number)
		if err != nil || rowsAffected == 0 {
			c.JSON(http.StatusConflict, gin.H{"error": "Failed to update local product (possible concurrency issue)"})
			return
		}

		// 5 - Return combined result
		c.JSON(http.StatusOK, gin.H{
			"LocalProduct": localProduct,
			"RemoteProduct": remoteProduct,
		})
	})

	router.Run(":3002") // Start API on port 3002
}

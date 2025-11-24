# Step 1: Build the Go binary
FROM golang:1.25.4-alpine AS builder

# Install git for fetching dependencies
RUN apk add --no-cache git

# Set working directory
WORKDIR /app

# Copy go.mod and go.sum first for caching
COPY ./GoAPI/go.mod ./GoAPI/go.sum ./

# Download dependencies
RUN go mod download

# Copy the rest of the source code
COPY . .

# Build the application (statically linked)
RUN CGO_ENABLED=0 GOOS=linux GOARCH=amd64 go build -o app ./GoAPI/main.go

# Step 2: Create a lightweight runtime image
FROM alpine:latest

# Set timezone and ca-certificates for HTTPS requests
RUN apk add --no-cache ca-certificates tzdata

# Set working directory
WORKDIR /app

# Copy the built binary from the builder
COPY --from=builder /app/app .

# Expose port 8080
EXPOSE 3002

# Run the binary
CMD ["./app"]

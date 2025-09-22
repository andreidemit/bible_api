#!/bin/bash

# Simple performance test script for the Bible API
# This demonstrates the caching and performance improvements

echo "=== Bible API Performance Test ==="
echo "Testing caching and response time improvements..."
echo ""

# Function to test endpoint performance
test_endpoint() {
    local url=$1
    local description=$2
    echo "Testing: $description"
    echo "URL: $url"
    
    # Test first request (cache miss)
    echo -n "First request (cache miss): "
    time_output=$(curl -s -w "%{time_total}" -o /dev/null "$url" 2>/dev/null)
    echo "${time_output}s"
    
    # Test second request (cache hit)
    echo -n "Second request (cache hit): "
    time_output=$(curl -s -w "%{time_total}" -o /dev/null "$url" 2>/dev/null)
    echo "${time_output}s"
    
    # Check response headers for caching
    echo "Response headers:"
    curl -s -I "$url" | grep -E "(cache-control|x-response-time)" || echo "No cache headers found"
    echo ""
}

# Check if API is running
if ! curl -s http://localhost:5000/healthz > /dev/null 2>&1; then
    echo "API is not running. Please start it first with:"
    echo "cd BibleApi && dotnet run --urls=http://localhost:5000"
    exit 1
fi

# Test health endpoint
echo "=== Health Check ==="
curl -s http://localhost:5000/healthz | jq '.' 2>/dev/null || curl -s http://localhost:5000/healthz
echo ""
echo ""

# Test various endpoints
test_endpoint "http://localhost:5000/v1/data" "List Translations"
test_endpoint "http://localhost:5000/v1/data/kjv" "Get Books for KJV (if available)"
test_endpoint "http://localhost:5000/v1/data/kjv/GEN" "Get Genesis Chapters (if available)"

echo "=== Performance Test Complete ==="
echo ""
echo "Expected improvements:"
echo "- Second requests should be faster due to caching"
echo "- Response headers should include cache-control directives"
echo "- X-Response-Time-Ms header should show request timing"
echo ""
echo "Note: Actual performance will depend on Azure Storage connectivity"
echo "In production with real data, the improvements will be more significant."
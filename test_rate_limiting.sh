#!/bin/bash

# Load test script to demonstrate rate limiting
echo "=== Bible API Rate Limiting Test ==="
echo "Testing rate limiting implementation..."
echo ""

# Check if API is running
if ! curl -s http://localhost:5000/healthz > /dev/null 2>&1; then
    echo "API is not running. Please start it first with:"
    echo "cd BibleApi && dotnet run --urls=http://localhost:5000"
    exit 1
fi

echo "Sending 105 requests rapidly to test rate limiting (limit: 100/minute)..."
echo ""

success_count=0
rate_limited_count=0

for i in {1..105}; do
    response=$(curl -s -w "%{http_code}" -o /dev/null http://localhost:5000/healthz)
    
    if [ "$response" = "200" ]; then
        success_count=$((success_count + 1))
        echo -n "."
    elif [ "$response" = "429" ]; then
        rate_limited_count=$((rate_limited_count + 1))
        echo -n "R"
    else
        echo -n "E"
    fi
    
    # Small delay to avoid overwhelming the system
    sleep 0.1
done

echo ""
echo ""
echo "Results:"
echo "Successful requests (200): $success_count"
echo "Rate limited requests (429): $rate_limited_count"
echo "Other responses: $((105 - success_count - rate_limited_count))"
echo ""

if [ $rate_limited_count -gt 0 ]; then
    echo "✅ Rate limiting is working correctly!"
    echo "Some requests were blocked with HTTP 429 status."
else
    echo "⚠️  Rate limiting may not be active or limit not reached."
    echo "This could be normal if running without the proper Azure connection."
fi

echo ""
echo "Rate limiting configuration:"
echo "- 100 requests per minute per IP"
echo "- 1000 requests per hour per IP"
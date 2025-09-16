#!/bin/bash

# Bible API Python Conversion - Quick Start Script
# This script helps you get the Python version running quickly

set -e  # Exit on any error

echo "🐍 Bible API - Python Version Quick Start"
echo "=========================================="

# Check if Python 3.12+ is available
PYTHON_CMD=""
for cmd in python3.12 python3 python; do
    if command -v "$cmd" >/dev/null 2>&1; then
        version=$($cmd --version 2>&1 | grep -oE '[0-9]+\.[0-9]+' | head -1)
        # Simple version check for Python 3.8+
        major=$(echo "$version" | cut -d. -f1)
        minor=$(echo "$version" | cut -d. -f2)
        if [[ $major -gt 3 ]] || [[ $major -eq 3 && $minor -ge 8 ]]; then
            PYTHON_CMD="$cmd"
            echo "✓ Found Python $version at $(command -v $cmd)"
            break
        fi
    fi
done

if [[ -z "$PYTHON_CMD" ]]; then
    echo "❌ Python 3.8+ is required but not found. Please install Python 3.8 or newer."
    exit 1
fi

# Check if pip is available
if ! command -v pip >/dev/null 2>&1 && ! $PYTHON_CMD -m pip --version >/dev/null 2>&1; then
    echo "❌ pip is required but not found. Please install pip."
    exit 1
fi

PIP_CMD="$PYTHON_CMD -m pip"

echo "✓ Using pip: $PIP_CMD"

# Install dependencies
echo ""
echo "📦 Installing Python dependencies..."
$PIP_CMD install -r requirements.txt

# Check if .env file exists
if [[ ! -f .env ]]; then
    echo ""
    echo "⚙️  Creating sample .env file..."
    cat > .env << EOF
# Azure Blob Storage configuration
AZURE_STORAGE_CONNECTION_STRING=your_azure_storage_connection_string
AZURE_STORAGE_ACCOUNT_NAME=your_account_name
AZURE_STORAGE_ACCOUNT_KEY=your_account_key
AZURE_CONTAINER_NAME=bible-translations

# Server configuration
PORT=8000
EOF
    echo "✓ Created .env file. Please update with your Azure Storage settings."
else
    echo "✓ .env file already exists"
fi

# Run basic tests
echo ""
echo "🧪 Running basic tests..."
$PYTHON_CMD test_conversion.py

echo ""
echo "🎉 Setup complete!"
echo ""
echo "Next steps:"
echo "1. Set up your Azure Blob Storage account and container"
echo "2. Update the .env file with your Azure Storage connection details"
echo "3. Upload Bible translation XML files to your Azure Storage container"
echo "4. Start the server: $PYTHON_CMD -m uvicorn main:app --reload --host 0.0.0.0 --port 8000"
echo ""
echo "📚 Documentation:"
echo "   • API docs (when running): http://localhost:8000/docs"
echo "   • ReDoc docs (when running): http://localhost:8000/redoc"
echo "   • Main page (when running): http://localhost:8000/"
echo ""
echo "🔄 Migration from Ruby:"
echo "   • The Python version maintains full API compatibility"
echo "   • All endpoints work exactly the same as the Ruby version"
echo "   • Data is now served from Azure Blob Storage instead of database"
echo "   • XML files are parsed dynamically for verses"
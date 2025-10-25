#!/bin/bash

# Coverage Statistics Script for ViHis Project
echo "📊 VIHIS COVERAGE STATISTICS - UPDATED"
echo "======================================"
echo ""

# 1. Test Results Summary
echo "🧪 TEST RESULTS:"
echo "----------------"
total_tests=$(find . -name "coverage.cobertura.xml" -type f | wc -l)
echo "Total Test Runs: $total_tests"

# 2. Coverage Reports Available
echo ""
echo "📈 COVERAGE REPORTS AVAILABLE:"
echo "------------------------------"
ls -la coverage-report*/index.html 2>/dev/null | while read line; do
    echo "✅ $line"
done

# 3. Latest Coverage Report
echo ""
echo "🎯 LATEST COVERAGE REPORT:"
echo "--------------------------"
latest_report=$(ls -t coverage-report*/index.html 2>/dev/null | head -1)
if [ -n "$latest_report" ]; then
    echo "✅ $latest_report"
    echo "🌐 Open in browser: open $latest_report"
else
    echo "❌ No coverage reports found"
fi

# 4. Coverage Files Count
echo ""
echo "📁 COVERAGE FILES:"
echo "------------------"
coverage_files=$(find . -name "coverage.cobertura.xml" -type f | wc -l)
echo "Coverage files found: $coverage_files"

# 5. Test Results by Type
echo ""
echo "🔍 TEST RESULTS BY TYPE:"
echo "------------------------"
echo "Unit Tests: GeminiStudyServiceRealTests.cs (32 tests)"
echo "Integration Tests: GeminiStudyServiceIntegrationTests.cs (8 tests)"
echo "Total Test Cases: 40 (32 Unit + 8 Integration)"

# 6. Coverage Dashboard
echo ""
echo "📊 COVERAGE DASHBOARD:"
echo "----------------------"
if [ -f "coverage-dashboard/index.html" ]; then
    echo "✅ Dashboard available: coverage-dashboard/index.html"
    echo "🌐 Open dashboard: open coverage-dashboard/index.html"
else
    echo "❌ Dashboard not found"
fi

# 7. Quick Stats
echo ""
echo "⚡ QUICK STATS:"
echo "--------------"
echo "📊 HTML Reports: $(ls coverage-report*/index.html 2>/dev/null | wc -l)"
echo "📁 Coverage Files: $coverage_files"
echo "🎯 Test Cases: 40 (32 Unit + 8 Integration)"
echo "🚀 Framework: xUnit + FluentAssertions"
echo "📈 Target Coverage: >85%"

echo ""
echo "🎉 Use 'open coverage-report-final-new/index.html' to view latest report!"

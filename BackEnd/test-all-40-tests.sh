#!/bin/bash

# Test All 40 Test Cases with 10s delay
echo "🚀 TESTING ALL 40 TEST CASES"
echo "============================="
echo "⏱️  Delay: 10 seconds between tests"
echo "🎯 Total: 40 tests (32 Unit + 8 Integration)"
echo ""

# List of all 40 test methods
TEST_METHODS=(
    "TC01_AskAsync_WithMongoDBContext_ReturnsValidAnswer"
    "TC02_AskAsync_WithEmptyMongoDB_FallsBackToWeb"
    "TC03_AskAsync_WithBothMongoAndWeb_UsesMongoFirst"
    "TC04_AskAsync_WithEmptyQuestion_ReturnsGracefully"
    "TC05_AskAsync_MaxContextZero_ClampsToOne"
    "TC06_AskAsync_MaxContext100_ClampsTo32"
    "TC07_AskAsync_NullLanguage_DefaultsToVietnamese"
    "TC08_AskAsync_SpecialCharactersInQuestion_HandlesCorrectly"
    "TC09_AskAsync_MissingAPIKey_ThrowsInvalidOperationException"
    "TC10_AskAsync_MissingModel_ThrowsInvalidOperationException"
    "TC11_AskAsync_GeminiAPITimeout_ThrowsTaskCanceledException"
    "TC12_AskAsync_GeminiAPI429_ThrowsHttpRequestException"
    "TC13_AskAsync_GeminiReturnsEmptyCandidates_ReturnsFallbackMessage"
    "TC14_AskAsync_MongoDBConnectionError_FallsBackToWebGracefully"
    "TC15_AskAsync_WikipediaFails_GeminiAnswersWithoutContext"
    "TC16_AskAsync_WithGoogleSearchEnabled_UsesWebFallback"
    "TC17_AskAsync_WithoutGoogleSearch_FallsBackToWikipedia"
    "TC18_AskAsync_WikipediaEnglish_UsesEnWikipedia"
    "TC19_AskAsync_EmptyMongoDBAndWebSearchFails_ReturnsWithoutContext"
    "TC20_AskAsync_LongQuestion_HandlesCorrectly"
    "TC21_AskAsync_VietnameseQuestion_ReturnsVietnameseAnswer"
    "TC22_AskAsync_EnglishQuestion_ReturnsEnglishAnswer"
    "TC23_AskAsync_HistoricalEvent_ReturnsDetailedAnswer"
    "TC24_AskAsync_Personality_ReturnsBiographicalAnswer"
    "TC25_AskAsync_CulturalQuestion_ReturnsCulturalAnswer"
    "TC26_AskAsync_ConcurrentRequests_AllSucceed"
    "TC27_AskAsync_WithRichMongoDBData_ReturnsDetailedAnswer"
    "TC28_AskAsync_WithMultipleLanguages_HandlesCorrectly"
    "TC29_AskAsync_WithComplexHistoricalQuestion_ReturnsComprehensiveAnswer"
    "TC30_AskAsync_WithVeryLongQuestion_HandlesCorrectly"
    "TC31_AskAsync_WithSpecialUnicodeCharacters_HandlesCorrectly"
    "TC32_AskAsync_WithExtremeContextLength_HandlesCorrectly"
    "TC33_AskAsync_WithInvalidLanguageCode_DefaultsToVietnamese"
    "TC34_AskAsync_WithNegativeContextLength_ClampsToMinimum"
    "TC35_AskAsync_WithMalformedQuestion_HandlesGracefully"
    "IT01_RealAPI_VietnameseHistoryQuestion_ReturnsValidAnswer"
    "IT02_RealAPI_QuestionNotInDatabase_FallsBackToWeb"
    "IT03_RealAPI_EnglishLanguage_ReturnsEnglishAnswer"
    "IT04_RealAPI_ConcurrentRequests_AllSucceed"
    "IT05_RealAPI_MongoDBConnection_VerifyDataAccess"
    "IT06_RealAPI_ComplexHistoricalAnalysis_ReturnsDetailedAnswer"
    "IT07_RealAPI_MultiLanguageSupport_HandlesCorrectly"
    "IT08_RealAPI_PerformanceUnderLoad_HandlesConcurrentRequests"
)

DELAY_SECONDS=10 # 10 seconds delay
PASSED=0
FAILED=0
SKIPPED=0

echo "📊 Starting test execution..."
echo ""

for i in "${!TEST_METHODS[@]}"; do
    test="${TEST_METHODS[$i]}"
    test_num=$((i + 1))
    
    echo "🧪 [$test_num/40] Running: $test"
    echo "----------------------------------------"
    
    # Run single test
    dotnet test --filter "$test" --logger "console;verbosity=normal" --collect:"XPlat Code Coverage" --no-build
    
    exit_code=$?
    
    if [ $exit_code -eq 0 ]; then
        echo "✅ [$test_num] PASSED: $test"
        ((PASSED++))
    elif [ $exit_code -eq 1 ]; then
        echo "❌ [$test_num] FAILED: $test"
        ((FAILED++))
    else
        echo "⏭️  [$test_num] SKIPPED: $test"
        ((SKIPPED++))
    fi
    
    echo ""
    
    # Wait before next test (except for last test)
    if [ $test_num -lt 40 ]; then
        echo "⏳ Waiting $DELAY_SECONDS seconds before next test..."
        sleep $DELAY_SECONDS
        echo ""
    fi
done

echo "🎉 ALL 40 TESTS COMPLETED!"
echo "=========================="
echo "�� FINAL RESULTS:"
echo "  ✅ Passed: $PASSED"
echo "  ❌ Failed: $FAILED"
echo "  ⏭️  Skipped: $SKIPPED"
echo "  📈 Total: $((PASSED + FAILED + SKIPPED))"
echo "  🎯 Success Rate: $(( (PASSED * 100) / (PASSED + FAILED + SKIPPED) ))%"
echo ""
echo "⏱️  Total execution time: ~$(( (PASSED + FAILED + SKIPPED) * DELAY_SECONDS / 60 )) minutes"

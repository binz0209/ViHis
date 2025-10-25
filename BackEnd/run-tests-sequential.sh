#!/bin/bash

# Sequential Test Runner for ViHis Project
echo "🚀 SEQUENTIAL TEST RUNNER STARTING..."
echo "====================================="

# List of all test methods (40 tests)
TEST_METHODS=(
    "VietHistory.AI.Tests.GeminiStudyServiceRealTests.TC01_AskAsync_WithMongoDBContext_ReturnsValidAnswer"
    "VietHistory.AI.Tests.GeminiStudyServiceRealTests.TC02_AskAsync_WithEmptyMongoDB_FallsBackToWeb"
    "VietHistory.AI.Tests.GeminiStudyServiceRealTests.TC03_AskAsync_WithBothMongoAndWeb_UsesMongoFirst"
    "VietHistory.AI.Tests.GeminiStudyServiceRealTests.TC04_AskAsync_WithEmptyQuestion_ReturnsGracefully"
    "VietHistory.AI.Tests.GeminiStudyServiceRealTests.TC05_AskAsync_MaxContextZero_ClampsToOne"
    "VietHistory.AI.Tests.GeminiStudyServiceRealTests.TC06_AskAsync_MaxContext100_ClampsTo32"
    "VietHistory.AI.Tests.GeminiStudyServiceRealTests.TC07_AskAsync_NullLanguage_DefaultsToVietnamese"
    "VietHistory.AI.Tests.GeminiStudyServiceRealTests.TC08_AskAsync_SpecialCharactersInQuestion_HandlesCorrectly"
    "VietHistory.AI.Tests.GeminiStudyServiceRealTests.TC09_AskAsync_MissingAPIKey_ThrowsInvalidOperationException"
    "VietHistory.AI.Tests.GeminiStudyServiceRealTests.TC10_AskAsync_MissingModel_ThrowsInvalidOperationException"
    "VietHistory.AI.Tests.GeminiStudyServiceRealTests.TC11_AskAsync_GeminiAPITimeout_ThrowsTaskCanceledException"
    "VietHistory.AI.Tests.GeminiStudyServiceRealTests.TC12_AskAsync_GeminiAPI429_ThrowsHttpRequestException"
    "VietHistory.AI.Tests.GeminiStudyServiceRealTests.TC13_AskAsync_GeminiReturnsEmptyCandidates_ReturnsFallbackMessage"
    "VietHistory.AI.Tests.GeminiStudyServiceRealTests.TC14_AskAsync_MongoDBConnectionError_FallsBackToWebGracefully"
    "VietHistory.AI.Tests.GeminiStudyServiceRealTests.TC15_AskAsync_WikipediaFails_GeminiAnswersWithoutContext"
    "VietHistory.AI.Tests.GeminiStudyServiceRealTests.TC16_AskAsync_WithGoogleSearchEnabled_UsesWebFallback"
    "VietHistory.AI.Tests.GeminiStudyServiceRealTests.TC17_AskAsync_WithoutGoogleSearch_FallsBackToWikipedia"
    "VietHistory.AI.Tests.GeminiStudyServiceRealTests.TC18_AskAsync_WikipediaEnglish_UsesEnWikipedia"
    "VietHistory.AI.Tests.GeminiStudyServiceRealTests.TC19_AskAsync_EmptyMongoDBAndWebSearchFails_ReturnsWithoutContext"
    "VietHistory.AI.Tests.GeminiStudyServiceRealTests.TC20_AskAsync_LongQuestion_HandlesCorrectly"
    "VietHistory.AI.Tests.GeminiStudyServiceRealTests.TC21_AskAsync_VietnameseQuestion_ReturnsVietnameseAnswer"
    "VietHistory.AI.Tests.GeminiStudyServiceRealTests.TC22_AskAsync_EnglishQuestion_ReturnsEnglishAnswer"
    "VietHistory.AI.Tests.GeminiStudyServiceRealTests.TC23_AskAsync_HistoricalEvent_ReturnsDetailedAnswer"
    "VietHistory.AI.Tests.GeminiStudyServiceRealTests.TC24_AskAsync_Personality_ReturnsBiographicalAnswer"
    "VietHistory.AI.Tests.GeminiStudyServiceRealTests.TC25_AskAsync_CulturalQuestion_ReturnsCulturalAnswer"
    "VietHistory.AI.Tests.GeminiStudyServiceRealTests.TC26_AskAsync_ConcurrentRequests_AllSucceed"
    "VietHistory.AI.Tests.GeminiStudyServiceRealTests.TC27_AskAsync_WithRichMongoDBData_ReturnsDetailedAnswer"
    "VietHistory.AI.Tests.GeminiStudyServiceRealTests.TC28_AskAsync_WithMultipleLanguages_HandlesCorrectly"
    "VietHistory.AI.Tests.GeminiStudyServiceRealTests.TC29_AskAsync_WithComplexHistoricalQuestion_ReturnsComprehensiveAnswer"
    "VietHistory.AI.Tests.GeminiStudyServiceRealTests.TC30_AskAsync_WithVeryLongQuestion_HandlesCorrectly"
    "VietHistory.AI.Tests.GeminiStudyServiceRealTests.TC31_AskAsync_WithSpecialUnicodeCharacters_HandlesCorrectly"
    "VietHistory.AI.Tests.GeminiStudyServiceRealTests.TC32_AskAsync_WithExtremeContextLength_HandlesCorrectly"
    "VietHistory.AI.Tests.GeminiStudyServiceRealTests.TC33_AskAsync_WithInvalidLanguageCode_DefaultsToVietnamese"
    "VietHistory.AI.Tests.GeminiStudyServiceRealTests.TC34_AskAsync_WithNegativeContextLength_ClampsToMinimum"
    "VietHistory.AI.Tests.GeminiStudyServiceRealTests.TC35_AskAsync_WithMalformedQuestion_HandlesGracefully"
    "VietHistory.AI.Tests.GeminiStudyServiceIntegrationTests.IT01_RealAPI_VietnameseHistoryQuestion_ReturnsValidAnswer"
    "VietHistory.AI.Tests.GeminiStudyServiceIntegrationTests.IT02_RealAPI_QuestionNotInDatabase_FallsBackToWeb"
    "VietHistory.AI.Tests.GeminiStudyServiceIntegrationTests.IT03_RealAPI_EnglishLanguage_ReturnsEnglishAnswer"
    "VietHistory.AI.Tests.GeminiStudyServiceIntegrationTests.IT04_RealAPI_ConcurrentRequests_AllSucceed"
    "VietHistory.AI.Tests.GeminiStudyServiceIntegrationTests.IT05_RealAPI_MongoDBConnection_VerifyDataAccess"
    "VietHistory.AI.Tests.GeminiStudyServiceIntegrationTests.IT06_RealAPI_ComplexHistoricalAnalysis_ReturnsDetailedAnswer"
    "VietHistory.AI.Tests.GeminiStudyServiceIntegrationTests.IT07_RealAPI_MultiLanguageSupport_HandlesCorrectly"
    "VietHistory.AI.Tests.GeminiStudyServiceIntegrationTests.IT08_RealAPI_PerformanceUnderLoad_HandlesConcurrentRequests"
)

DELAY_SECONDS=30 # Delay between tests

echo "📊 Total Tests: ${#TEST_METHODS[@]}"
echo "⏱️  Delay between tests: ${DELAY_SECONDS}s"
echo ""

PASSED=0
FAILED=0

for i in "${!TEST_METHODS[@]}"; do
    test="${TEST_METHODS[$i]}"
    test_num=$((i + 1))
    
    echo "�� [$test_num/${#TEST_METHODS[@]}] Running: $test"
    echo "------------------------------------------------------------"
    
    # Run single test
    dotnet test --filter "$test" --logger "console;verbosity=normal" --collect:"XPlat Code Coverage"
    
    if [ $? -eq 0 ]; then
        echo "✅ [$test_num] PASSED: $test"
        ((PASSED++))
    else
        echo "❌ [$test_num] FAILED: $test"
        ((FAILED++))
    fi
    
    echo ""
    
    # Wait before next test (except for last test)
    if [ $test_num -lt ${#TEST_METHODS[@]} ]; then
        echo "⏳ Waiting $DELAY_SECONDS seconds before next test..."
        sleep $DELAY_SECONDS
        echo ""
    fi
done

echo "🎉 SEQUENTIAL TEST EXECUTION COMPLETED!"
echo "======================================"
echo "📊 RESULTS:"
echo "  ✅ Passed: $PASSED"
echo "  ❌ Failed: $FAILED"
echo "  �� Total: $((PASSED + FAILED))"
echo "  🎯 Success Rate: $(( (PASSED * 100) / (PASSED + FAILED) ))%"

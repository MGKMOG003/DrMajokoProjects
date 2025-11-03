using Microsoft.AspNetCore.Mvc;
using DrMajokoProjects.API.Services.Interfaces;
using DrMajokoProjects.API.Models.Entities;

namespace DrMajokoProjects.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SurveysController : ControllerBase
    {
        private readonly IFirebaseService _firebaseService;
        private readonly ILogger<SurveysController> _logger;

        public SurveysController(IFirebaseService firebaseService, ILogger<SurveysController> logger)
        {
            _firebaseService = firebaseService;
            _logger = logger;
        }

        [HttpGet("contractor/{contractorId}")]
        public async Task<ActionResult<List<ContractorSurvey>>> GetContractorSurveys(string contractorId)
        {
            try
            {
                var surveys = await _firebaseService.QueryCollectionAsync<ContractorSurvey>("surveys", "ContractorId", contractorId);
                return Ok(surveys.OrderByDescending(s => s.CreatedAt).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving surveys for contractor {contractorId}");
                return StatusCode(500, new { error = "Failed to retrieve surveys" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ContractorSurvey>> CreateSurvey([FromBody] ContractorSurvey survey)
        {
            try
            {
                survey.CreatedAt = DateTime.UtcNow;
                survey.OverallRating = (survey.TimelinessRating + survey.QualityRating +
                                       survey.CommunicationRating + survey.ProfessionalismRating) / 4.0;

                var surveyId = await _firebaseService.AddDocumentAsync("surveys", survey);
                survey.Id = surveyId;

                return Ok(survey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating survey");
                return StatusCode(500, new { error = "Failed to create survey" });
            }
        }
    }
}
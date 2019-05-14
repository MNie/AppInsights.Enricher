namespace AppInsights.Enricher.WebApi.Controllers
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;
    using Request.Filters;

    public class TestController : Controller
    {
        public class ComplicatedSubclass
        {
            public string Field1 { get; set; }
            [Sensitive] public string Field2 { get; set; }
        }
        public class ComplicatedContract
        {
            [Sensitive]
            public int Field1 { get; set; }
            public IReadOnlyCollection<int> Field2 { get; set; }
            public string Field3 { get; set; }
            public ComplicatedSubclass Field4 { get; set; }
        }
        
        [HttpPost("500")]
        public IActionResult Post500([FromBody] ComplicatedContract contract) =>
            StatusCode(500);
        
        [HttpPost("400")]
        public IActionResult Post400([FromBody] ComplicatedContract contract) =>
            StatusCode(400);
        
        [HttpPost("404")]
        public IActionResult Post404([FromBody] ComplicatedContract contract) =>
            StatusCode(404);
        
        [HttpPost("200")]
        public IActionResult Post200([FromBody] ComplicatedContract contract) =>
            Ok(new ComplicatedContract());
    }
}
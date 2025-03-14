using Microsoft.AspNetCore.Mvc;

namespace CancellationTokens.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CancellationTokenController(IBlogRepository blogRepository) : ControllerBase
    {
        private readonly IBlogRepository _blogRepository = blogRepository;

        [SupportsRequestCancellation]
        [HttpGet("supports-cancellation")]
        public async Task<ActionResult<Blog[]>> GetSupportsCancellation() =>
            await _blogRepository.GetBlogs();

        [HttpGet("does-not-support-cancellation")]
        public async Task<ActionResult<Blog[]>> GetDoesNotSupportCancellation() =>
            await _blogRepository.GetBlogs();
    }
}

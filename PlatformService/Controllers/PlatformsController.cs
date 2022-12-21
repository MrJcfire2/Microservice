using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.Dtos;
using PlatformService.Models;
using PlatformService.SyncDataServices.Http;

namespace PlatformService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlatformsController : ControllerBase
    {
        private readonly IPlatformRepo _repository;
        private readonly IMapper _mapper;
        private readonly ICommandDataClient _commandDataClient;
        private readonly IMessageBusClient _messageBusClient;

        public PlatformsController(
            IPlatformRepo repository,
            IMapper mapper,
            ICommandDataClient commandDataClient,
            IMessageBusClient messageBusClient)
        {
            _repository = repository;
            _mapper = mapper;
            _commandDataClient = commandDataClient;
            _messageBusClient = messageBusClient;
        }

        [HttpGet]
        public ActionResult<IEnumerable<PlatformReadDto>> GetPlatforms() {
            var platforms = _repository.GetAllPlatforms();
            return Ok(_mapper.Map<IEnumerable<PlatformReadDto>>(platforms));
        }

        [HttpGet("{platformId}", Name = "GetPlatformById")]
        public ActionResult<PlatformReadDto> GetPlatformById(int platformId) {
            var platform = _repository.GetPlatformById(platformId);
            if(platform == null) return NotFound($"No platform was found for id: {platformId}");
            return Ok(_mapper.Map<PlatformReadDto>(platform));
        }

        [HttpPost]
        public async Task<ActionResult<PlatformReadDto>> CreatePlatform(PlatformCreateDto platformCreateDto) {
            var platform = _mapper.Map<Platform>(platformCreateDto);
            _repository.CreatePlatform(platform);
            _repository.SaveChanges();

            var platformReadDto = _mapper.Map<PlatformReadDto>(platform);

            // Send Sync Message
            try
            {
                await _commandDataClient.SendPlatformToCommand(platformReadDto);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"--> Could not send synchronous: {ex.Message}");
            }

            // Send Async Message
            try
            {
                var publishedPlatformDto = _mapper.Map<PlatformPublishedDto>(platformReadDto);
                publishedPlatformDto.Event = "Platform_Published";
                _messageBusClient.PublishNewPlatform(publishedPlatformDto);
            }
            catch(Exception ex) {
                Console.WriteLine($"--> Could not send synchronous: {ex.Message}");
            }
            
            return CreatedAtRoute(nameof(GetPlatformById), new { PlatformId = platformReadDto.Id}, platformReadDto);
        }
    }
}
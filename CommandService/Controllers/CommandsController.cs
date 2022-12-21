using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using CommandsService.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommandsService.Controllers
{
    [Route("api/c/platforms/{platformId}/[controller]")]
    [ApiController]
    public class CommandsController : ControllerBase
    {
        private readonly ICommandRepo _repostitory;
        private readonly IMapper _mapper;

        public CommandsController(ICommandRepo repository, IMapper mapper)
        {
            _repostitory = repository;
            _mapper = mapper;
        }

        [HttpGet]
        public ActionResult<IEnumerable<CommandReadDto>> GetCommandsForPlatform(int platformId)
        {
            Console.WriteLine($"--> Hit GetCommandsForPlatform {platformId}");

            if(!_repostitory.PlatformExists(platformId)) return NotFound($"No platform with id: {platformId}");
            
            var commandItems = _repostitory.GetCommandsForPlatform(platformId);

            return Ok(_mapper.Map<IEnumerable<CommandReadDto>>(commandItems));
        }

        [HttpGet("{commandId}", Name = "GetCommandForPlatform")]
        public ActionResult<CommandReadDto> GetCommandForPlatform(int platformId, int commandId)
        {
            Console.WriteLine($"--> Hit GetCommandForPlatform {platformId} / {commandId}");

            if(!_repostitory.PlatformExists(platformId)) return NotFound($"No platform with id: {platformId}");

            var command = _repostitory.GetCommand(platformId, commandId);

            if(command == null) return NotFound($"No command with id: {commandId}");

            return Ok(_mapper.Map<CommandReadDto>(command));
        }

        [HttpPost]
        public ActionResult<CommandReadDto> CreateCommandForPlatform(int platformId, [FromBody] CommandCreateDto commandDto)
        {
            Console.WriteLine($"--> Hit CreateCommandForPlatform {platformId}");
            
            if(!_repostitory.PlatformExists(platformId)) return NotFound($"No platform with id: {platformId}");

            var command = _mapper.Map<Command>(commandDto);

            _repostitory.CreateCommand(platformId, command);
            _repostitory.SaveChanges();

            var commandReadDto = _mapper.Map<CommandReadDto>(command);

            return CreatedAtRoute(nameof(GetCommandForPlatform),
                new {platformId = platformId, commandId = commandReadDto.Id}, commandReadDto);
        }
    }
}
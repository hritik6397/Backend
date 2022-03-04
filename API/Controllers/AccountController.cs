using Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using API.Dtos;
using API.Errors;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using API.Extensions;
using AutoMapper;
using Infrastructure.Services;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Web;
using API.service;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManger;
        private readonly SignInManager<AppUser> _signInManger;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly IEmailSender _emailSender;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
                        ITokenService tokenService, IMapper mapper, IEmailSender emailSender)
        {
            _emailSender = emailSender;
            _mapper = mapper;
            _tokenService = tokenService;
            _userManger = userManager;
            _signInManger = signInManager;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var user = await _userManger.FindByEmailClaimsPrinciple(HttpContext.User);
            return new UserDto
            {
                Email = user.Email,
                DisplayName = user.DisplayName,
                Token = _tokenService.CreateToken(user)

            };
        }

        [HttpGet("emailexists")]
        public async Task<ActionResult<bool>> CheckEmailExistsAsync([FromQuery] string email)
        {
            return await _userManger.FindByEmailAsync(email) != null;
        }


        [Authorize]
        [HttpGet("address")]
        public async Task<ActionResult<AddressDto>> GetUserAddress()
        {

            var user = await _userManger.FindByEmailWithAddressAsync(HttpContext.User);
            return _mapper.Map<Address, AddressDto>(user.Address);
        }

        [Authorize]
        [HttpPut("address")]     //Updating address
        public async Task<ActionResult<AddressDto>> UpdateUserAddress(AddressDto address)
        {
            var user = await _userManger.FindByEmailWithAddressAsync(HttpContext.User);
            user.Address = _mapper.Map<AddressDto, Address>(address);
            var result = await _userManger.UpdateAsync(user);
            if (result.Succeeded) return Ok(_mapper.Map<Address, AddressDto>(user.Address));   //pass updated user
            return BadRequest("Problem in updating the user");

        }


        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _userManger.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                return Unauthorized(new ApiResponse(401));
            }
            if (!await _userManger.IsEmailConfirmedAsync(user))
            {
                return Unauthorized("Email is not confirmed");
            }
            var result = await _signInManger.CheckPasswordSignInAsync(user, loginDto.Password, false);


            if (!result.Succeeded) return Unauthorized(new ApiResponse(401));


            return new UserDto
            {
                Email = loginDto.Email,
                DisplayName = user.DisplayName,
                Token = _tokenService.CreateToken(user)

            };
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (CheckEmailExistsAsync(registerDto.Email).Result.Value)
            {
                return new BadRequestObjectResult(new ApiValidationErrorResponse { Errors = new[] { "Email address is in Use" } });
            }
            var user = new AppUser
            {
                DisplayName = registerDto.DisplayName,
                Email = registerDto.Email,
                UserName = registerDto.Email

            };
            //  await SendEmailAsync(registerDto.Email , "hi" , "hello");
            var result = await _userManger.CreateAsync(user, registerDto.Password);

            if (result.Succeeded)
            {
                var token = await _userManger.GenerateEmailConfirmationTokenAsync(user);
                var param = new Dictionary<string, string>
                {
                   {"token", token },
                   {"email", user.Email }
                };

                var callback = QueryHelpers.AddQueryString(registerDto.clienturi, param);
                var message = new Message(new string[] { user.Email }, "Email Confirmation token", callback, null);
                await _emailSender.SendEmailAsync(message);
            }

            if (!result.Succeeded) return BadRequest(new ApiResponse(400));



            return new UserDto
            {
                DisplayName = user.DisplayName,
                Token = _tokenService.CreateToken(user),
                Email = user.Email
            };
        }

        [HttpGet("EmailConfirmation")]
       
       public async Task<IActionResult> EmailConfirmation(string email, string token)
       {
           //[FromQuery]string email,[FromQuery]string token
         var user = await _userManger.FindByEmailAsync(email);
        if (user == null)
        {
        return BadRequest("Invalid Email Confirmation Request");
        }
        var confirmResult = await _userManger.ConfirmEmailAsync(user,token);
         if (!confirmResult.Succeeded)
         {
            
        return BadRequest("Invalid Email Confirmation Request-----");
         }
        return Ok();
       }
     



    }
}
﻿namespace Identity.Api.Dto;

public class CreateUserRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}
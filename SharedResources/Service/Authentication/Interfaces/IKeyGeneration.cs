﻿namespace DataAccess.Service.Authentication.Interfaces
{
    public interface IKeyGeneration
    {
        byte[] GenerateRandomKey(int keyLength);
    }
}

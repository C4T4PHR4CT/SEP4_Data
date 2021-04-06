﻿namespace SEP4_Data.Data
{
    public interface IConfigService
    {
        public int Port { get; set; }
        public bool Https { get; set; }
        public string DbHost { get; set; }
        public int DbPort { get; set; }
        public string DbName { get; set; }
        public string DbUser { get; set; }
        public string DbPassword { get; set; }
        public byte[] JwtKey { get; set; }
        public byte[] Salt { get; set; }
        public bool CheckIntegrityOnStartup { get; set; }
        public int TokenExpire { get; set; }
        public bool Swagger { get; set; }
    }
}
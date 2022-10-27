﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Data.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ServerDatabase
{
    public class ServerDbContext : DbContext
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Chat> Chats => Set<Chat>();
        public DbSet<Message> Messages => Set<Message>();
        public DbSet<File> Files => Set<File>();
        public DbSet<UserChatRelative> UserChatRelatives => Set<UserChatRelative>();
        
        public ServerDbContext()
        {

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=tcp:user-chat.database.windows.net,1433;Initial Catalog=User-Chat-Database;Persist Security Info=False;User ID=AdminLogin;Password=AdminPassword!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserChatRelative>().HasKey(ucr => new { ucr.UserId, ucr.ChatId });
        }
    }
}
﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Skuld.Core.Models;

namespace Skuld.Core.Models.Migrations
{
    [DbContext(typeof(SkuldDatabaseContext))]
    partial class SkuldDatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.0-preview1.19506.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Skuld.Core.Generic.Models.SkuldConfig", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("AltPrefix")
                        .HasColumnType("longtext");

                    b.Property<string>("B4DToken")
                        .HasColumnType("longtext");

                    b.Property<string>("DBotsOrgKey")
                        .HasColumnType("longtext");

                    b.Property<ulong>("DailyAmount")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("DataDogHost")
                        .HasColumnType("longtext");

                    b.Property<ushort?>("DataDogPort")
                        .HasColumnType("smallint unsigned");

                    b.Property<string>("DiscordGGKey")
                        .HasColumnType("longtext");

                    b.Property<string>("DiscordToken")
                        .HasColumnType("longtext");

                    b.Property<string>("GoogleAPI")
                        .HasColumnType("longtext");

                    b.Property<string>("GoogleCx")
                        .HasColumnType("longtext");

                    b.Property<string>("ImgurClientID")
                        .HasColumnType("longtext");

                    b.Property<string>("ImgurClientSecret")
                        .HasColumnType("longtext");

                    b.Property<bool>("IsDevelopmentBuild")
                        .HasColumnType("bit");

                    b.Property<string>("NASAApiKey")
                        .HasColumnType("longtext");

                    b.Property<int>("PinboardDateLimit")
                        .HasColumnType("int");

                    b.Property<int>("PinboardThreshold")
                        .HasColumnType("int");

                    b.Property<string>("Prefix")
                        .HasColumnType("longtext");

                    b.Property<string>("STANDSToken")
                        .HasColumnType("longtext");

                    b.Property<int>("STANDSUid")
                        .HasColumnType("int");

                    b.Property<ushort>("Shards")
                        .HasColumnType("smallint unsigned");

                    b.Property<string>("TwitchClientID")
                        .HasColumnType("longtext");

                    b.Property<string>("TwitchToken")
                        .HasColumnType("longtext");

                    b.Property<float>("VoiceExpDeterminate")
                        .HasColumnType("float");

                    b.Property<ulong>("VoiceExpMaxGrant")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("VoiceExpMinMinutes")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("WebsocketHost")
                        .HasColumnType("longtext");

                    b.Property<ushort>("WebsocketPort")
                        .HasColumnType("smallint unsigned");

                    b.Property<bool>("WebsocketSecure")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.ToTable("Configurations");
                });

            modelBuilder.Entity("Skuld.Core.Models.BlockedAction", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("Blockee")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("Blocker")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.ToTable("BlockedActions");
                });

            modelBuilder.Entity("Skuld.Core.Models.CustomCommand", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Content")
                        .HasColumnType("longtext");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("Name")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("CustomCommands");
                });

            modelBuilder.Entity("Skuld.Core.Models.Guild", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("JoinChannel")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("JoinMessage")
                        .HasColumnType("longtext");

                    b.Property<ulong>("JoinRole")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("LeaveChannel")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("LeaveMessage")
                        .HasColumnType("longtext");

                    b.Property<int>("LevelNotification")
                        .HasColumnType("int");

                    b.Property<ulong>("LevelUpChannel")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("LevelUpMessage")
                        .HasColumnType("longtext");

                    b.Property<string>("MoneyIcon")
                        .HasColumnType("longtext");

                    b.Property<string>("MoneyName")
                        .HasColumnType("longtext");

                    b.Property<ulong>("MutedRole")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("Prefix")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Guilds");
                });

            modelBuilder.Entity("Skuld.Core.Models.GuildFeatures", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned");

                    b.Property<bool>("Experience")
                        .HasColumnType("bit");

                    b.Property<bool>("Pinning")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.ToTable("Features");
                });

            modelBuilder.Entity("Skuld.Core.Models.GuildModules", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned");

                    b.Property<bool>("Accounts")
                        .HasColumnType("bit");

                    b.Property<bool>("Actions")
                        .HasColumnType("bit");

                    b.Property<bool>("Admin")
                        .HasColumnType("bit");

                    b.Property<bool>("Custom")
                        .HasColumnType("bit");

                    b.Property<bool>("Fun")
                        .HasColumnType("bit");

                    b.Property<bool>("Gambling")
                        .HasColumnType("bit");

                    b.Property<bool>("Information")
                        .HasColumnType("bit");

                    b.Property<bool>("Lewd")
                        .HasColumnType("bit");

                    b.Property<bool>("Search")
                        .HasColumnType("bit");

                    b.Property<bool>("Space")
                        .HasColumnType("bit");

                    b.Property<bool>("Stats")
                        .HasColumnType("bit");

                    b.Property<bool>("Weeb")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.ToTable("Modules");
                });

            modelBuilder.Entity("Skuld.Core.Models.IAmRole", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("bigint unsigned");

                    b.Property<int>("LevelRequired")
                        .HasColumnType("int");

                    b.Property<int>("Price")
                        .HasColumnType("int");

                    b.Property<ulong>("RequiredRoleId")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("RoleId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.ToTable("IAmRoles");
                });

            modelBuilder.Entity("Skuld.Core.Models.LevelRewards", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("bigint unsigned");

                    b.Property<int>("LevelRequired")
                        .HasColumnType("int");

                    b.Property<ulong>("RoleId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.ToTable("LevelRewards");
                });

            modelBuilder.Entity("Skuld.Core.Models.Pasta", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("Content")
                        .HasColumnType("longtext");

                    b.Property<ulong>("Created")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("Name")
                        .HasColumnType("longtext");

                    b.Property<ulong>("OwnerId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.ToTable("Pastas");
                });

            modelBuilder.Entity("Skuld.Core.Models.PastaVotes", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("PastaId")
                        .HasColumnType("bigint unsigned");

                    b.Property<bool>("Upvote")
                        .HasColumnType("bit");

                    b.Property<ulong>("VoterId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.ToTable("PastaVotes");
                });

            modelBuilder.Entity("Skuld.Core.Models.Reputation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<ulong>("Repee")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("Reper")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("Timestamp")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.ToTable("Reputations");
                });

            modelBuilder.Entity("Skuld.Core.Models.User", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("AvatarUrl")
                        .HasColumnType("longtext");

                    b.Property<string>("Background")
                        .HasColumnType("longtext");

                    b.Property<string>("BanReason")
                        .HasColumnType("longtext");

                    b.Property<string>("Description")
                        .HasColumnType("longtext");

                    b.Property<ulong>("Flags")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("Language")
                        .HasColumnType("longtext");

                    b.Property<ulong>("LastDaily")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("Money")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("Pats")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("Patted")
                        .HasColumnType("bigint unsigned");

                    b.Property<bool>("RecurringBlock")
                        .HasColumnType("bit");

                    b.Property<string>("Title")
                        .HasColumnType("longtext");

                    b.Property<bool>("UnlockedCustBG")
                        .HasColumnType("bit");

                    b.Property<string>("Username")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Skuld.Core.Models.UserCommandUsage", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("Command")
                        .HasColumnType("longtext");

                    b.Property<ulong>("Usage")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("UserId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.ToTable("UserCommandUsage");
                });

            modelBuilder.Entity("Skuld.Core.Models.UserExperience", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("LastGranted")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("Level")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("TotalXP")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("UserId")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("XP")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.ToTable("UserXp");
                });
#pragma warning restore 612, 618
        }
    }
}

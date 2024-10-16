﻿using Dapper;
using MdsCommon;
using Metapsi.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace MdsBuildManager
{
    public class HashHelper
    {
        private readonly InputArguments inputArguments;

        public HashHelper(InputArguments inputArguments)
        {
            this.inputArguments = inputArguments;
        }

        public string GetProjectFilesHash(MemoryStream projectArchiveStream)
        {
            projectArchiveStream.Seek(0, SeekOrigin.Begin);
            ZipArchive projectArchive = new ZipArchive(projectArchiveStream);

            List<uint> allCrcs = new List<uint>();

            foreach (var projectFile in projectArchive.Entries.OrderBy(x => x.Name))
            {
                allCrcs.Add(projectFile.Crc32);
            }

            using (var md5 = MD5.Create())
            {
                string joinedCrcs = string.Join(',', allCrcs.Select(x => x.ToString()));
                var asciiJoinedCrcs = System.Text.Encoding.ASCII.GetBytes(joinedCrcs);
                var hash = md5.ComputeHash(asciiJoinedCrcs);
                var base64String = Convert.ToBase64String(hash);
                return base64String;
            }
            throw new Exception("Archived file cannot be hashed!");
        }

        //private string GetHashesDbPath()
        //{
        //    //return System.IO.Path.Combine(inputArguments.BinariesFolder, "MdsBuildManager.db");
        //}

        public static bool BuildAlreadyChecked(List<MdsBuildManager.Build> knownBuilds, int buildId, string version, string commitSha)
        {
            return knownBuilds.Any(x => x.BuildId == buildId && x.Version == version && x.CommitSha == commitSha);
        }

        public async Task<List<Binaries>> GetBinariesData(SqliteQueue sqliteQueue)
        {
            return await sqliteQueue.WithRollback(async t =>
            {
                var binaries = await t.Connection.QueryAsync<Binaries>("select * from Binaries", transaction: t);
                return binaries.ToList();
            });
        }

        public async Task<List<Build>> GetBuildData(SqliteQueue sqliteQueue)
        {
            return await sqliteQueue.WithRollback(async t =>
            {
                var build = await t.Connection.QueryAsync<Build>("select * from Build", transaction: t);
                return build.ToList();
            });
        }

        public async Task AddNewVersion(System.Data.IDbTransaction dbTransaction, string tag, string buildNumber, string commitSha, string projectName, string version, int buildId, string base64Hash, string target)
        {
            await dbTransaction.Connection.ExecuteAsync("insert into Build (Id,Tag,BuildNumber,CommitSha, ProjectName, Version, BuildId, Base64Hash, Timestamp, Target) values (@Id,@Tag,@BuildNumber,@CommitSha, @ProjectName, @Version, @BuildId, @Base64Hash, @Timestamp, @Target)",
                new { Id = Guid.NewGuid().ToString(), Tag = tag, BuildNumber = buildNumber, CommitSha = commitSha, ProjectName = projectName, BuildId = buildId, Base64Hash = base64Hash, Timestamp = DateTime.UtcNow.ToString("O"), Version = version, Target = target  });
        }

        public async Task AddNewBinaries(System.Data.IDbTransaction dbTransaction, string base64Hash, string binaryPath)
        {
            await dbTransaction.Connection.ExecuteAsync("insert into Binaries (Id, Base64Hash,BinaryPath) values (@Id, @Base64Hash,@BinaryPath)",
                new { Id = Guid.NewGuid().ToString(), Base64Hash = base64Hash, BinaryPath = binaryPath }, dbTransaction);
        }

        public async Task<int> DeleteBinaries(System.Data.IDbTransaction dbTransaction, AlgorithmInfo algorithmInfo)
        {
            return await dbTransaction.Connection.ExecuteAsync("delete from Build where ProjectName = @ProjectName and Version = @Version and Target = @Target",
                new { ProjectName = algorithmInfo.Name, Version = algorithmInfo.Version, Target = algorithmInfo.Target }, dbTransaction);
        }

        public static async Task<Metapsi.ServiceDoc.DbQueue> GetDbQueue(string fullDbPath)
        {
            if (!System.IO.File.Exists(fullDbPath))
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fullDbPath));
            }

            SqliteQueue sqliteQueue = new SqliteQueue(fullDbPath);
            await sqliteQueue.WithCommit(async t =>
            {
                await t.Connection.ExecuteAsync(BinariesTable, transaction: t);
                await t.Connection.ExecuteAsync(BuildTable, transaction: t);
            });

            return new Metapsi.ServiceDoc.DbQueue(sqliteQueue);
        }

        private const string BuildTable =
            @"CREATE TABLE IF NOT EXISTS ""Build"" (
	            ""Id""	TEXT NOT NULL,
	            ""Tag""	TEXT,
	            ""BuildNumber""	TEXT,
	            ""CommitSha""	TEXT,
	            ""ProjectName""	TEXT,
	            ""Version""	TEXT,
	            ""BuildId""	INTEGER,
	            ""Base64Hash""	TEXT,
	            ""Timestamp""	TEXT,
	            ""Target""	TEXT,
	            PRIMARY KEY(""Id"")
            )";

        private const string BinariesTable =
            @"CREATE TABLE IF NOT EXISTS ""Binaries"" (
	        ""Id""	TEXT,
	        ""Base64Hash""	TEXT,
	        ""BinaryPath""	TEXT
        )";
    }
}

using System;
using Metapsi;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Data.SQLite;
using Dapper;

namespace MdsLocal
{
    public static partial class MdsLocalApplication
    {
        public static async Task CheckAllServiceDbs(CommandContext commandContext, State state)
        {
            string servicesDbFolder = state.BaseDataFolder;

            if (string.IsNullOrEmpty(servicesDbFolder))
                return;

            if (!System.IO.Directory.Exists(servicesDbFolder))
                return;

            string[] allServiceFolders = System.IO.Directory.GetDirectories(servicesDbFolder);
            foreach (string serviceDbDirectory in allServiceFolders)
            {
                var dir = new System.IO.DirectoryInfo(serviceDbDirectory);
                string serviceName = dir.Name;
                string logDbFile = Mds.GetServiceDataFile(servicesDbFolder, serviceName, Mds.Constant.LogDbFile);
                commandContext.Logger.LogDebug(logDbFile);
                if (System.IO.File.Exists(logDbFile))
                {
                    await CheckServiceDbLogMessages(commandContext, state, logDbFile, serviceName);
                }
            }
        }


        public static LogEntry GetLastDbLogError(string dbPath, DateTime serviceStart)
        {
            if (!System.IO.File.Exists(dbPath))
                return null;

            string roundtrip = serviceStart.Roundtrip();

            using (var connection = new SQLiteConnection($"Data Source = {dbPath}"))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();
                LogEntry last = connection.QuerySingleOrDefault<LogEntry>("select * from Log where ProcessStartTimestamp=@startTimestamp and (LogMessageType = 'Error' or LogMessageType = 'Exception' or LogMessageType = 'StartupError')  order by date(logtimestamp) desc limit 1 ;", new { startTimestamp = serviceStart }, transaction: transaction);
                if (last == null)
                {
                    transaction.Rollback();
                    return null;
                }

                if (last.Processed == 0)
                {
                    int updated = transaction.Connection.Execute("update Log set Processed=1 where Id = @Id", last, transaction: transaction);
                    transaction.Commit();
                }
                else
                {
                    transaction.Rollback();
                }
                return last;
            }
        }

        public static async Task CheckServiceDbLogMessages(
            CommandContext commandContext,
            State state,
            string dbPath,
            string serviceName)
        {
            List<LogEntry> notProcessed = new List<LogEntry>();
            using (var connection = new SQLiteConnection($"Data Source = {dbPath}"))
            {
                await connection.OpenAsync();
                var transaction = await connection.BeginTransactionAsync();

                var tableExists = await transaction.Connection.QueryAsync("SELECT name FROM sqlite_master WHERE type='table' AND name='Log';", transaction: transaction);
                if (tableExists.Any())
                {
                    notProcessed.AddRange(await transaction.Connection.QueryAsync<LogEntry>("select * from Log where Processed=0 order by Id", transaction: transaction));
                }
                await transaction.RollbackAsync();
            }

            foreach (LogEntry logMessage in notProcessed)
            {
                switch (logMessage.LogMessageType)
                {
                    case nameof(Mds.Ping):
                        {
                            commandContext.PostEvent(new Event.ServicePing()
                            {
                                ServiceName = serviceName
                            });
                        }
                        break;
                    case nameof(Mds.StartupError):
                        {
                            Mds.StartupError startupError = new Mds.StartupError()
                            {
                                ErrorMessage = logMessage.LogMessage
                            };

                            state.DroppedServices.Add(serviceName);
                            commandContext.PostEvent(new Event.StartupError()
                            {
                                ServiceName = serviceName,
                                ErrorMessage = startupError.ErrorMessage
                            });
                        }
                        break;

                    //case nameof(Mds.FatalError):
                    //    {
                    //        // TODO: Not implemented yet. This should intentionally crash the service, then await for several seconds
                    //        //Mds.FatalError startupError = Ubiquitous.Serialize.FromJson<Mds.FatalError>(logMessage.LogData);
                    //    }
                    //    break;
                    case nameof(Metapsi.Log.Error):
                    case nameof(Metapsi.Log.Exception):
                        {
                            commandContext.PostEvent(new Event.Error()
                            {
                                ServiceName = serviceName,
                                ErrorMessage = $"{logMessage.CallStack}\n{logMessage.LogMessage}"
                            });
                        }
                        break;
                    case nameof(Metapsi.Log.Info):
                        {
                            commandContext.PostEvent(new Event.Info()
                            {
                                ServiceName = serviceName,
                                InfoMessage = $"{logMessage.CallStack}\n{logMessage.LogMessage}"
                            });
                        }
                        break;
                }
            }

            if (notProcessed.Any())
            {
                using (var connection = new SQLiteConnection($"Data Source = {dbPath}"))
                {
                    await connection.OpenAsync();
                    var transaction = await connection.BeginTransactionAsync();

                    int updated = await transaction.Connection.ExecuteAsync("update Log set Processed=1 where Id <= @Id", notProcessed.Last(), transaction: transaction);

                    int currentMax = await connection.ExecuteScalarAsync<int>("select max(Id) from Log", transaction: transaction);
                    int cutoffId = currentMax - 500;
                    await connection.ExecuteAsync("delete from Log where Id <= @cutoffId", new { cutoffId }, transaction);

                    await transaction.CommitAsync();
                    Console.WriteLine($"Found {notProcessed.Count} new log messages, processed {updated}");
                }
            }
        }

        public static async Task CheckServiceDbLogMessages(
           CommandContext commandContext,
           State state,
           string serviceName)
        {
            string dbPath = Mds.GetServiceLogDbFile(state.BaseDataFolder, serviceName);

            if (string.IsNullOrEmpty(dbPath))
                return;

            if (!System.IO.File.Exists(dbPath))
                return;

            await CheckServiceDbLogMessages(commandContext, state, dbPath, serviceName);
        }

        //public static Task WriteShutdownCommandToServiceDb(CommandContext commandContext, State state, string serviceName)
        //{
        //    return Mds.CommandToServiceDb(Mds.GetServiceDbPath(serviceName), new Mds.Shutdown() { });
        //}
    }
}

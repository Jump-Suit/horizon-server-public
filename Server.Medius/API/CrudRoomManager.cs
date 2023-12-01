using Newtonsoft.Json;
using Server.Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Medius.API
{
    public class CrudRoomManager
    {
        private static List<Room> rooms = new();

        // Update or Create a Room based on the provided parameters
        public static void UpdateOrCreateRoom(string appId, string gameName, string worldId, string accountName, string languageType, bool host)
        {
            Room? roomToUpdate = rooms.FirstOrDefault(r => r.AppId == appId);

            if (roomToUpdate == null)
            {
                roomToUpdate = new Room { AppId = appId, Worlds = new List<World>() };
                rooms.Add(roomToUpdate);
            }

            if(worldId != null)
            {
                World? worldToUpdate = roomToUpdate.Worlds?.FirstOrDefault(w => w.WorldId == worldId);

                if (worldToUpdate == null && worldId != "")
                {
                    worldToUpdate = new World { WorldId = worldId, GameSessions = new List<GameList>() };
                    roomToUpdate.Worlds?.Add(worldToUpdate);
                }

                GameList? gameToUpdate = worldToUpdate.GameSessions?.FirstOrDefault(w => w.Name == gameName);

                if (gameToUpdate == null && gameName != "")
                {
                    gameToUpdate = new GameList { Name = gameName, Clients = new List<Player>() };
                    worldToUpdate.GameSessions?.Add(gameToUpdate);
                }

                Player? playerToUpdate = gameToUpdate.Clients?.FirstOrDefault(p => p.Name == accountName);

                if (playerToUpdate == null && !string.IsNullOrEmpty(gameToUpdate.Name) && accountName != "" && languageType != "")
                {
                    if (gameToUpdate.Name.Contains("AP|"))
                    {
                        Player? playerToUpdatehashed = gameToUpdate.Clients?.FirstOrDefault(p => p.Name == Utils.ComputeSHA512ReducedSizeCustom(accountName));
                        if (playerToUpdatehashed == null)
                        {
                            playerToUpdate = new Player { Name = Utils.ComputeSHA512ReducedSizeCustom(accountName), Languages = languageType, Host = host };
                            gameToUpdate.Clients?.Add(playerToUpdate);
                        }
                    }
                    else
                    {
                        playerToUpdate = new Player { Name = accountName, Languages = languageType, Host = host };
                        gameToUpdate.Clients?.Add(playerToUpdate);
                    }
                }
                else
                {
                    playerToUpdate.Host = host;
                    playerToUpdate.Languages = languageType;
                }
            }

            
        }

        // Remove a user from a specific room based on the provided parameters
        public static void RemoveUser(string appId, string gameName, string worldId, string accountName)
        {
            var roomToRemoveUser = rooms.FirstOrDefault(r => r.AppId == appId);

            if (roomToRemoveUser != null)
            {
                var WorldToRemoveUser = roomToRemoveUser.Worlds?.FirstOrDefault(w => w.WorldId == worldId);

                if (WorldToRemoveUser != null)
                {
                    var GameToRemoveUser = WorldToRemoveUser.GameSessions?.FirstOrDefault(w => w.Name == gameName);

                    if (GameToRemoveUser != null && !string.IsNullOrEmpty(GameToRemoveUser.Name))
                    {
                        if (GameToRemoveUser.Name.Contains("AP|"))
                            GameToRemoveUser.Clients?.RemoveAll(p => p.Name == Utils.ComputeSHA512ReducedSizeCustom(accountName));
                        else
                            GameToRemoveUser.Clients?.RemoveAll(p => p.Name == accountName);
                    }
                }
            }
        }

        // Remove a world from a specific room based on the provided parameters
        public static Task RemoveWorld(string appId, string worldId)
        {
            var roomToRemove = rooms.FirstOrDefault(r => r.AppId == appId);

            if (roomToRemove != null)
            {
                var gameToRemove = roomToRemove.Worlds?.FirstOrDefault(w => w.WorldId == worldId);

                if (gameToRemove != null)
                    roomToRemove.Worlds?.RemoveAll(w => w.WorldId == worldId);
            }

            return Task.CompletedTask;
        }

        // Remove a game from a specific room based on the provided parameters
        public static Task RemoveGame(string appId, string worldId, string? gameName)
        {
            if (!string.IsNullOrEmpty(gameName))
            {
                var roomToRemove = rooms.FirstOrDefault(r => r.AppId == appId);

                if (roomToRemove != null)
                {
                    var gameToRemove = roomToRemove.Worlds?.FirstOrDefault(w => w.WorldId == worldId);

                    if (gameToRemove != null)
                    {
                        var worldToRemove = gameToRemove.GameSessions?.FirstOrDefault(w => w.Name == gameName);

                        if (worldToRemove != null)
                            gameToRemove.GameSessions?.RemoveAll(w => w.Name == gameName);
                    }
                }
            }

            return Task.CompletedTask;
        }

        // Remove a Room by AppId
        public static void RemoveRoom(string appId)
        {
            rooms.RemoveAll(r => r.AppId == appId);
        }

        // Get a list of all Rooms
        public static List<Room> GetAllRooms()
        {
            return rooms;
        }

        // Serialize the RoomConfig to JSON
        public static string ToJson()
        {
            return JsonConvert.SerializeObject(rooms, Formatting.Indented);
        }
    }

    public class Room
    {
        public string? AppId { get; set; }
        public List<World>? Worlds { get; set; }
    }

    public class World
    {
        public string? WorldId { get; set; }
        public List<GameList>? GameSessions { get; set; }
    }

    public class GameList
    {
        public string? Name { get; set; }
        public List<Player>? Clients { get; set; }
    }

    public class Player
    {
        public bool Host { get; set; }
        public string? Name { get; set; }
        public string? Languages { get; set; }
    }
}

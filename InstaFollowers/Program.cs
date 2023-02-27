using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using InstagramApiSharp;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Logger;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Enums;
using Org.BouncyCastle.Asn1;
using System.Configuration;

namespace InstaFollowers
{

    class Program
    {
        /// <summary>
        ///     Api instance (one instance per Instagram user)
        /// </summary>
        static void Main(string[] args)
        {
            var result = Task.Run(MainAsync).GetAwaiter().GetResult();
            if (result)
                return;
            Console.ReadKey();
        }
        public static async Task<bool> MainAsync()
        {

            Console.WriteLine("Starting demo of InstagramApiSharp project");


            // create user session data and provide login details....
            var userSession = new UserSessionData
            {
                UserName = GetUserInput("Enter your Instagram username: "),
                Password = GetUserInput("Enter your Instagram password: ")
            };

            InstagramApiService _instagramApiService = new InstagramApiService(userSession);


            await _instagramApiService.ownLogin();

            Console.WriteLine("\nTrayendo lista de seguidores...\n");

            var user = await _instagramApiService.GetUser(userSession.UserName);

            IResult < InstaUserShortList> usersFollow = await _instagramApiService.GetFollowers(user.Value.Pk);

            Dictionary<string, long> seguidoresNombre = new Dictionary<string, long>();

            listSort(usersFollow, seguidoresNombre);

            Console.WriteLine("\nYa traje los seguidores");

            Console.WriteLine("\nTrayendo lista de seguidos...\n");

            IResult<InstaUserShortList> userFollowings = await _instagramApiService.GetFollowing(user.Value.Pk);

            Dictionary<string, long> seguidosNombre = new Dictionary<string, long>();

            listSort(userFollowings, seguidosNombre);

            Console.WriteLine("\nYa traje los seguidos");

            Console.WriteLine("CANTIDAD DE SEGUIDORES QUE RECIBÍ: " + seguidoresNombre.Count);

            Console.WriteLine("CANTIDAD DE SEGUIDOS QUE RECIBÍ: " + seguidosNombre.Count);


            int opcion;

            do
            {
                Console.WriteLine("1. Ver seguidores");
                Console.WriteLine("2. Ver quiénes sigo que no me siguen y al revés");
                Console.WriteLine("3. Ver lista de seguidores de antes");
                Console.WriteLine("4. Renovar lista de seguidores");
                Console.WriteLine("5. Comparar lista de seguidores");
                Console.WriteLine("6. Chusmear otro usuario");

                Console.WriteLine("Elija una opción");


                opcion = int.Parse(Console.ReadLine());

                switch (opcion)
                {
                    case 1:

                        Console.WriteLine("Seguidores: ");


                        foreach (var m in seguidoresNombre)
                        {
                            Console.WriteLine(m.Key);
                        }

                        break;

                    case 2:

                        Console.WriteLine("\n\n\n\n\n\nSiguiendo: ");

                        WriteAndError(userFollowings, seguidosNombre);

                        Console.WriteLine("\n\n\n\n\n\nSiguidores: ");

                        /*foreach (var m in seguidoresNombre)
                        {
                            Console.WriteLine(m);
                        }*/

                        WriteAndError(usersFollow, seguidoresNombre);

                        FollowFollowingDiff(seguidoresNombre, seguidosNombre);

                        break;

                    case 3:

                        string[] viejos = File.ReadAllLines("lista1.txt");

                        foreach (var x in viejos)
                            Console.WriteLine(x);
                        break;

                    case 4:

                        File.WriteAllLines("lista1.txt", seguidoresNombre.Select(kv => $"{kv.Key},{kv.Value}"));
                        Console.WriteLine("Lista actualizada!");
                        break;

                    case 5:
                        var oldFollowers = new Dictionary<string, long>();
                        foreach (var line in File.ReadAllLines("lista1.txt"))
                        {
                            var parts = line.Split(',');
                            if (parts.Length == 2 && long.TryParse(parts[1], out long pk))
                            {
                                oldFollowers[parts[0]] = pk;
                            }
                        }
                        OldNewDiff(oldFollowers, seguidoresNombre);

                        break;

                    case 6:
                        int ajeno = 1;
                        do
                        {
                            Console.WriteLine("\nA quién chusmeamos??");
                            string usuario = Console.ReadLine();

                            var user2 = await _instagramApiService.GetUser(usuario);

                            var usersFollow2 = await _instagramApiService.GetFollowers(user2.Value.Pk);

                            Dictionary<string, long> seguidoresNombre2 = new Dictionary<string, long>();

                            listSort(usersFollow2, seguidoresNombre2);

                            Console.WriteLine("Seguidores: ");

                            WriteAndError(usersFollow2, seguidoresNombre2);

                            Console.ReadKey();

                            Console.WriteLine(Environment.NewLine);

                            Console.WriteLine("\n\n\n\n\n\nSiguiendo: ");

                            var userFollowing2 = await _instagramApiService.GetFollowing(user2.Value.Pk);

                            Dictionary<string, long> seguidosNombre2 = new Dictionary<string, long>();

                            listSort(userFollowing2, seguidosNombre2);

                            WriteAndError(userFollowing2, seguidosNombre2);

                            FollowFollowingDiff(seguidoresNombre2, seguidosNombre2);

                            Console.WriteLine("CANTIDAD DE SEGUIDORES QUE RECIBÍ: " + seguidoresNombre2.Count);

                            Console.WriteLine("CANTIDAD DE SEGUIDOS QUE RECIBÍ: " + seguidosNombre2.Count);

                            Console.WriteLine("1. Seguir chusmeando otros usuarios\n2. Volver al usuario principal.");
                            if (Console.ReadLine() == "2")
                                ajeno = 0;
                        } while (ajeno == 1);

                        usersFollow = await _instagramApiService.GetFollowers(user.Value.Pk);

                        listSort(usersFollow, seguidoresNombre);

                        userFollowings = await _instagramApiService.GetFollowing(user.Value.Pk);

                        listSort(userFollowings, seguidosNombre);

                        break;
                }

                Console.ReadKey();
                Console.Clear();
            } while (opcion != 0);

            return false;

        }

        private static void FollowFollowingDiff(Dictionary<string, long> seguidoresDict, Dictionary<string, long> seguidosDict)
        {
            var seguidoresNoSiguenQuery = getDifferences(seguidosDict, seguidoresDict);

            var seguidosNoSiguenQuery = getDifferences(seguidoresDict, seguidosDict);

            Console.WriteLine("\n\n\n\n\n\n\n\nGente que te sigue que no seguís: ");
            foreach (string s in seguidosNoSiguenQuery)
            {
                Console.WriteLine(s);
            }

            Console.WriteLine("\n\n\n\n\n\n\n\nGente que seguís que no te sigue: ");
            foreach (string s in seguidoresNoSiguenQuery)
            {
                Console.WriteLine(s);
            }
        }

        private static void OldNewDiff(Dictionary<string, long> OldFollowersDict, Dictionary<string, long> NewFollowersDict)
        {
            var DeadFollowersQuery = getDifferences(OldFollowersDict, NewFollowersDict);

            var NewFollowersQuery = getDifferences(NewFollowersDict, OldFollowersDict); 

            Console.WriteLine("\n\n\n\n\n\n\n\nNuevos seguidores: ");
            foreach (string s in NewFollowersQuery)
            {
                Console.WriteLine(s);
            }

            Console.WriteLine("\n\n\n\n\n\n\n\nTe dejaron de seguir: ");
            foreach (string s in DeadFollowersQuery)
            {
                Console.WriteLine(s);
            }
        }

        private static IEnumerable<string> getDifferences(Dictionary<string, long> Dict1, Dictionary<string, long> Dict2)
        {
            var diff = Dict1.Where(x => !Dict2.ContainsValue(x.Value)).Select(x => x.Key);

            return diff;
        }

        private static void WriteAndError(IResult<InstaUserShortList> UsersList, Dictionary<string, long> UsersNames)
        {
            int numDuplicates = 0;

            foreach (var m in UsersNames)
            {
                Console.WriteLine(m.Key);
            }

            foreach (var m in UsersList.Value)
            {
                try 
                {
                    UsersNames.Add(m.UserName, m.Pk);
                }
                catch(ArgumentException ex) {
                    numDuplicates =+ 1;
                }
            }


            Console.WriteLine("Errores: " + numDuplicates);
        }

        private static void listSort(IResult<InstaUserShortList> friendsList, Dictionary<string, long> friendsDict)
        {

            int numDuplicates = 0;

            foreach (var m in friendsList.Value)
            {

                try
                { 
                    friendsDict.Add(m.UserName, m.Pk);
                }
                catch (ArgumentException ex)
                {
                    numDuplicates =+ 1;
                }
            }

            Console.WriteLine("Errores: " + numDuplicates);
            var orderedDict = friendsDict.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            friendsDict = orderedDict;

        }

        private static string GetUserInput(string message)
        {
            Console.Write(message);
            return Console.ReadLine();
        }

    }
    public class InstagramApiService
    {
        private readonly UserSessionData _userSession;
        private readonly IInstaApi _instaApi;
        public InstagramApiService(UserSessionData userSession)
        {
            _userSession = userSession;

            var delay = RequestDelay.FromSeconds(2, 2);
            // create new InstaApi instance using Builder
            _instaApi = InstaApiBuilder.CreateBuilder()
                .SetUser(_userSession)
                //.UseLogger(new DebugLogger(LogLevel.All)) // use logger for requests and debug messages
                .SetRequestDelay(delay)
                .Build();
        }

        public async Task<UserSessionData> ownLogin()
        {
            var userSession = new UserSessionData();

            do
            {

                var delay = RequestDelay.FromSeconds(2, 2);

                if (!_instaApi.IsUserAuthenticated)
                {
                    // login
                    Console.WriteLine($"Logging in as {userSession.UserName}");
                    delay.Disable();
                    var logInResult = await Login();
                    delay.Enable();
                    if (!logInResult.Succeeded)
                    {
                        Console.WriteLine($"Unable to login: {logInResult.Info.Message}");
                        Console.WriteLine("Check login details and try again.");
                        Console.WriteLine("Enter your Instagram username: ");
                        userSession.UserName = Console.ReadLine();
                        Console.WriteLine("Enter your Instagram password: ");
                        userSession.Password = Console.ReadLine();

                    }
                }
            } while (!_instaApi.IsUserAuthenticated);


            Console.WriteLine($"Logged in as {userSession.UserName}");
            return userSession;
        }


        public async Task<IResult<InstaUserShortList>> GetFollowers(long userid)
        {
            return await _instaApi.UserProcessor.GetUserFollowersByIdAsync(userid, PaginationParameters.Empty);
        }

        public async Task<IResult<InstaUserShortList>> GetFollowing(long userid)
        {
            return await _instaApi.UserProcessor.GetUserFollowingByIdAsync(userid, PaginationParameters.Empty);
        }

        public async Task<IResult<InstaLoginResult>> Login()
        {
            return await _instaApi.LoginAsync();
        }

        public async Task<IResult<InstaUser>> GetUser(string username)
        {
            return await _instaApi.UserProcessor.GetUserAsync(username);
        }
    }
}


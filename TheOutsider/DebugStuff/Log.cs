using OWML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TheOutsider
{
    public static class Log
    {
        static IModConsole Console;
        public static void Initialize(IModConsole console)
        {
            Console = console;
        }

        //--------------------------------------------- Printing for testing, not final version. ---------------------------------------------//
        public static void Print(object message) {
            if (ModMain.isDevelopmentVersion) Console.WriteLine($"{message}");
        }
        public static void Print(string line) {
            if (ModMain.isDevelopmentVersion) Console.WriteLine(line);
        }
        public static void Success(string line, bool showInFinal = true) {
            if (ModMain.isDevelopmentVersion || showInFinal) Console.WriteLine(line, MessageType.Success);
        }
        //--------------------------------------------- Warnings and Errors should show always. ---------------------------------------------//
        public static void Warning(string line) => Console.WriteLine(line, MessageType.Warning);
        public static void Error(string line) => Console.WriteLine(line, MessageType.Error);
        public static bool ErrorIf(bool error, string errorMessage)
        {
            if (error)
            {
                Console.WriteLine(errorMessage, MessageType.Error);
                return true;
            }
            return false;
        }

        //--------------------------------------------- Misc ---------------------------------------------//
        public static void PrintAllComponentsUnder(GameObject gameObject)
        {
            if (!ModMain.isDevelopmentVersion) return;

            var components = gameObject.GetComponentsInChildren<Component>();
            foreach (var c in components)
            {
                bool e = false;
                if (c is Behaviour b) e = b.enabled;
                if (c is Collider coll) e = coll.enabled;

                Console.WriteLine($"{c} | {e}");
            }
        }
        public static void PrintAllPositionsUnder(GameObject gameObject)
        {
            if (!ModMain.isDevelopmentVersion) return;

            var components = gameObject.GetComponentsInChildren<Transform>();
            foreach (var c in components)
            {
                Console.WriteLine($"{c.gameObject.name}: {c.localPosition} -> {c.parent}");
            }
        }
    }
}
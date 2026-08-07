using System;
using System.Reflection;
using Verse;

namespace TaskInterrupt.Compatibility
{
    /// <summary>
    /// Bridges the message and confirmation surfaces whose namespaces and
    /// overloads changed before the modern RimWorld UI API settled.
    /// </summary>
    internal static class TaskInterruptUiApi
    {
        private const BindingFlags StaticMembers =
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags InstanceMembers =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        internal static void ShowMessage(string text, string messageTypeName)
        {
            Assembly gameAssembly = typeof(Pawn).Assembly;
            Type messagesType = gameAssembly.GetType("Verse.Messages");
            if (messagesType == null)
            {
                return;
            }

            object messageType = ReadMessageType(gameAssembly, messageTypeName);
            MethodInfo[] methods = messagesType.GetMethods(StaticMembers);
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];
                if (method.Name != "Message")
                {
                    continue;
                }

                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length < 2 ||
                    parameters[0].ParameterType != typeof(string) ||
                    messageType == null ||
                    !parameters[1].ParameterType.IsInstanceOfType(messageType))
                {
                    continue;
                }

                object[] arguments = BuildArguments(
                    parameters,
                    text,
                    messageType,
                    null);
                try
                {
                    method.Invoke(null, arguments);
                    return;
                }
                catch (TargetInvocationException)
                {
                    return;
                }
            }
        }

        internal static bool Confirm(string text, Action confirmed)
        {
            Assembly gameAssembly = typeof(Pawn).Assembly;
            Type dialogType = gameAssembly.GetType("Verse.Dialog_MessageBox");
            if (dialogType == null)
            {
                return false;
            }

            MethodInfo[] methods = dialogType.GetMethods(StaticMembers);
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];
                if (method.Name != "CreateConfirmation")
                {
                    continue;
                }

                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length < 2 ||
                    parameters[0].ParameterType != typeof(string) ||
                    !typeof(Delegate).IsAssignableFrom(parameters[1].ParameterType))
                {
                    continue;
                }

                object[] arguments = new object[parameters.Length];
                arguments[0] = text;
                arguments[1] = confirmed;
                for (int j = 2; j < parameters.Length; j++)
                {
                    arguments[j] = parameters[j].ParameterType == typeof(bool)
                        ? (object)false
                        : GetDefault(parameters[j].ParameterType);
                }

                object dialog;
                try
                {
                    dialog = method.Invoke(null, arguments);
                }
                catch (TargetInvocationException)
                {
                    return false;
                }

                if (dialog == null)
                {
                    return false;
                }

                object windowStack = ReadStaticMember(
                    gameAssembly.GetType("Verse.Find"),
                    "WindowStack");
                if (windowStack == null)
                {
                    return false;
                }

                MethodInfo add = FindAddMethod(windowStack.GetType());
                if (add == null)
                {
                    return false;
                }

                add.Invoke(windowStack, new[] { dialog });
                return true;
            }

            return false;
        }

        private static object[] BuildArguments(
            ParameterInfo[] parameters,
            string text,
            object messageType,
            Action ignored)
        {
            object[] arguments = new object[parameters.Length];
            arguments[0] = text;
            if (parameters.Length > 1)
            {
                arguments[1] = messageType;
            }
            for (int i = 2; i < parameters.Length; i++)
            {
                Type parameterType = parameters[i].ParameterType;
                arguments[i] = parameterType == typeof(bool)
                    ? (object)false
                    : GetDefault(parameterType);
            }
            return arguments;
        }

        private static object ReadMessageType(
            Assembly gameAssembly,
            string messageTypeName)
        {
            Type defs = gameAssembly.GetType("RimWorld.MessageTypeDefOf");
            if (defs == null)
            {
                return null;
            }

            PropertyInfo property = defs.GetProperty(messageTypeName, StaticMembers);
            if (property != null)
            {
                return property.GetValue(null, null);
            }

            FieldInfo field = defs.GetField(messageTypeName, StaticMembers);
            return field?.GetValue(null);
        }

        private static object ReadStaticMember(Type type, string name)
        {
            if (type == null)
            {
                return null;
            }

            PropertyInfo property = type.GetProperty(name, StaticMembers);
            if (property != null)
            {
                return property.GetValue(null, null);
            }

            FieldInfo field = type.GetField(name, StaticMembers);
            return field?.GetValue(null);
        }

        private static MethodInfo FindAddMethod(Type type)
        {
            MethodInfo[] methods = type.GetMethods(InstanceMembers);
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];
                ParameterInfo[] parameters = method.GetParameters();
                if (method.Name == "Add" && parameters.Length == 1)
                {
                    return method;
                }
            }

            return null;
        }

        private static object GetDefault(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
}

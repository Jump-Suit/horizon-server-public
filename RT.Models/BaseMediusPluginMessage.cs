using RT.Common;
using Server.Common.Logging;
using Server.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace RT.Models
{

    #region BaseMediusPluginMessage
    public abstract class BaseMediusPluginMessage
    {
        /// <summary>
        /// Message class.
        /// </summary>
        public abstract NetMessageClass PacketClass { get; }

        /// <summary>
        /// Message type.
        /// </summary>
        public abstract NetMessageTypeIds PacketType { get; }

        /// <summary>
        /// When true, skips encryption when sending this particular message instance.
        /// </summary>
        public virtual bool SkipEncryption { get; set; } = false;

        public BaseMediusPluginMessage()
        {

        }

        #region Serialization

        /// <summary>
        /// Deserializes the plugin message from plaintext.
        /// </summary>
        /// <param name="reader"></param>
        public virtual void DeserializePlugin(Server.Common.Stream.MessageReader reader)
        {

        }

        /// <summary>
        /// Serialize contents of the plugin message.
        /// </summary>
        public virtual void SerializePlugin(Server.Common.Stream.MessageWriter writer)
        {

        }

        #endregion

        #region Logging

        /// <summary>
        /// Whether or not this message passes the log filter.
        /// </summary>
        public virtual bool CanLog()
        {

            switch (PacketClass)
            {
                case NetMessageClass.MessageClassApplication: return LogSettings.Singleton?.IsLogPlugin(PacketType) ?? false;
                default: return true;
            }

        }

        #endregion

        #region Dynamic Instantiation

        private static Dictionary<NetMessageTypeIds, Type> _netPluginMessageTypeById = null;

        private static int _messageClassByIdLockValue = 0;
        private static object _messageClassByIdLockObject = _messageClassByIdLockValue;


        private static void Initialize()
        {
            lock (_messageClassByIdLockObject)
            {

                _netPluginMessageTypeById = new Dictionary<NetMessageTypeIds, Type>();

                // Populate
                var assembly = System.Reflection.Assembly.GetAssembly(typeof(BaseMediusPluginMessage));
                var types = assembly.GetTypes();


                foreach (Type classType in types)
                {
                    // Objects by Id
                    var attrs = (MediusMessageAttribute[])classType.GetCustomAttributes(typeof(MediusMessageAttribute), true);
                    if (attrs != null && attrs.Length > 0)
                    {
                        switch (attrs[0].MessageClass)
                        {
                            case NetMessageClass.MessageClassApplication:
                                {
                                    _netPluginMessageTypeById.Add((NetMessageTypeIds)attrs[0].MessageType, classType);
                                    break;
                                }
                        }

                    }
                }
            }
        }

        public static BaseMediusPluginMessage Instantiate(Server.Common.Stream.MessageReader reader)
        {
            BaseMediusPluginMessage msg;

            Type classType = null;

            var msgClass = reader.Read<NetMessageClass>();
            reader.ReadBytes(2);
            var msgType = reader.Read<NetMessageTypeIds>();


            // Init
            Initialize();

            switch (msgClass)
            {
                case NetMessageClass.MessageClassApplication:
                    {
                        if (!_netPluginMessageTypeById.TryGetValue(msgType, out classType))
                            classType = null;
                        break;
                    }
            }

            // Instantiate
            if (classType == null)
                msg = new RawMediusMessage0(msgClass, msgType);
            else
                msg = (BaseMediusPluginMessage)Activator.CreateInstance(classType);

            // Deserialize
            msg.DeserializePlugin(reader);
            return msg;

            /*
            // Instantiate
            if (classType == null)
                msg = new RawMediusMessage0((byte)msgType);
            else
                msg = (BaseMediusPluginMessage)Activator.CreateInstance(classType);

            // Deserialize
            msg.Deserialize(reader);
            return msg;
            */
        }

        #endregion

    }
    #endregion
}

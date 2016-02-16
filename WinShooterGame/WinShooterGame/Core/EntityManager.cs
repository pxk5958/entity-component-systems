using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace WinShooterGame.Core
{
    class EntityManager
    {
        /*
        private static Dictionary<Int64, IList<Func<Int64, bool>>> componentSubsystemRegisterMap = new Dictionary<Int64, IList<Func<Int64, bool>>>();
        public static Dictionary<Int64, IList<Func<Int64, bool>>> ComponentSubsystemRegisterMap
        {
            get { return componentSubsystemRegisterMap; }
        }

        private static Dictionary<Int64, IList<Func<Int64, bool>>> componentSubsystemUnregisterMap = new Dictionary<Int64, IList<Func<Int64, bool>>>();
        public static Dictionary<Int64, IList<Func<Int64, bool>>> ComponentSubsystemUnregisterMap
        {
            get { return componentSubsystemUnregisterMap; }
        }
        */

        private static SQLiteConnection m_dbConnection;
        public static SQLiteConnection DBConnection
        {
            get { return m_dbConnection; }
        }

        //private static Dictionary<string, Int64> componentNameIdMap = new Dictionary<string, Int64>();
        /*
        private static Dictionary<string, string> componentNameTableNameMap = new Dictionary<string, string>();
        public static Dictionary<string, string> ComponentNameTableNameMap
        {
            get { return componentNameTableNameMap; }
        }
        */

        public static bool InitializeDatabase()
        {
            bool exists = false;

            m_dbConnection = new SQLiteConnection("Data Source=:memory:; PRAGMA synchronous=OFF; PRAGMA journal_mode=MEMORY");
            m_dbConnection.Open();

            string sourcePath = "Content\\WinShooterGame.sqlite";
            string savePath = "Content\\save.sqlite";
            /*
            if (File.Exists(savePath))
            {
                sourcePath = savePath;
                exists = true;
            }
            */

            using (SQLiteConnection source = new SQLiteConnection("Data Source=" + sourcePath))
            {
                source.Open();

                source.BackupDatabase(m_dbConnection, "main", "main", -1, null, 0);
            }

            return exists;
        }

        /*
        private static readonly Finalizer finalizer = new Finalizer();

        private sealed class Finalizer
        {
            ~Finalizer()
            {
            }
        }
        */

        public static void FinalizeDatabase()
        {
            SaveDatabase();
            m_dbConnection.Close();
        }

        public static void SaveDatabase()
        {
            string backupFileName = "Content\\save.sqlite";
            SQLiteConnection.CreateFile(backupFileName);
            using (SQLiteConnection backup = new SQLiteConnection("Data Source=" + backupFileName))
            {
                backup.Open();

                m_dbConnection.BackupDatabase(backup, "main", "main", -1, null, 0);
            }
        }

        private EntityManager() { }

        public static Int64 CreateEntity(string label = null)
        {
            Int64 retEntityId = 0;

            SQLiteCommand insertEntityCommand = new SQLiteCommand("INSERT INTO _entities (label) VALUES (@label); SELECT Last_Insert_Rowid();", m_dbConnection);
            insertEntityCommand.Parameters.Add(new SQLiteParameter("@label", label));
            retEntityId = (Int64)insertEntityCommand.ExecuteScalar();

            return retEntityId;
        }

        public static Int64 CreateEntity(Int64 assemblageId, string label = null)
        {
            Int64 retEntityId = 0;

            retEntityId = CreateEntity(label);

            AddComponents(retEntityId, assemblageId);

            return retEntityId;
        }

        public static Int64 CreateEntity(string assemblageName, string label = null)
        {
            Int64 retEntityId = 0;

            SQLiteCommand getAssemblageIdCommand = new SQLiteCommand("SELECT assemblage_id from _assemblages WHERE official_name=@official_name", m_dbConnection);
            getAssemblageIdCommand.Parameters.Add(new SQLiteParameter("@official_name", assemblageName));
            SQLiteDataReader getAssemblageIdReader = getAssemblageIdCommand.ExecuteReader();
            if (getAssemblageIdReader.Read())
            {
                Int64 assemblageId = (Int64)getAssemblageIdReader["assemblage_id"];
                retEntityId = CreateEntity(assemblageId, label);
            }

            return retEntityId;
        }

        public static void AddComponent(Int64 entityId, Int64 componentId, string parameters = null, params object[] values)
        {
            SQLiteCommand getComponentTableCommand = new SQLiteCommand("SELECT table_name from _components WHERE component_id=@component_id", m_dbConnection);
            getComponentTableCommand.Parameters.Add(new SQLiteParameter("@component_id", componentId));
            SQLiteDataReader getComponentTableReader = getComponentTableCommand.ExecuteReader();
            if (getComponentTableReader.Read())
            {
                if (getComponentTableReader["table_name"] != DBNull.Value)
                {
                    string componentTable = (string)getComponentTableReader["table_name"];

                    Int64 componentDataId = AddComponentData(componentTable, parameters, values);
                    if (componentDataId > 0)
                    {
                        SQLiteCommand insertEntityComponentCommand = new SQLiteCommand("INSERT INTO _entity_components (entity_id, component_id, component_data_id) VALUES (@entity_id,@component_id,@component_data_id)", m_dbConnection);
                        insertEntityComponentCommand.Parameters.Add(new SQLiteParameter("@entity_id", entityId));
                        insertEntityComponentCommand.Parameters.Add(new SQLiteParameter("@component_id", componentId));
                        insertEntityComponentCommand.Parameters.Add(new SQLiteParameter("@component_data_id", componentDataId));
                        insertEntityComponentCommand.ExecuteNonQuery();
                    }
                }
                else
                {
                    SQLiteCommand insertEntityComponentCommand = new SQLiteCommand("INSERT INTO _entity_components (entity_id, component_id) VALUES (@entity_id,@component_id)", m_dbConnection);
                    insertEntityComponentCommand.Parameters.Add(new SQLiteParameter("@entity_id", entityId));
                    insertEntityComponentCommand.Parameters.Add(new SQLiteParameter("@component_id", componentId));
                    insertEntityComponentCommand.ExecuteNonQuery();
                }

                /*
                if (componentSubsystemRegisterMap.ContainsKey(componentId))
                {
                    foreach (Func<Int64, bool> func in componentSubsystemRegisterMap[componentId])
                    {
                        func(entityId);
                    }
                }
                */
            }
        }

        public static void AddComponent(Int64 entityId, string componentName, string parameters = null, params object[] values)
        {
            SQLiteCommand getComponentTableCommand = new SQLiteCommand("SELECT component_id, table_name from _components WHERE official_name = @component_name", m_dbConnection);
            getComponentTableCommand.Parameters.Add(new SQLiteParameter("@component_name", componentName));
            SQLiteDataReader getComponentTableReader = getComponentTableCommand.ExecuteReader();
            if (getComponentTableReader.Read())
            {
                Int64 componentId = (Int64)getComponentTableReader["component_id"];

                if (getComponentTableReader["table_name"] != DBNull.Value)
                {
                    string componentTable = (string)getComponentTableReader["table_name"];

                    Int64 componentDataId = AddComponentData(componentTable, parameters, values);
                    if (componentDataId > 0)
                    {
                        SQLiteCommand insertEntityComponentCommand = new SQLiteCommand("INSERT INTO _entity_components (entity_id, component_id, component_data_id) VALUES (@entity_id,@component_id,@component_data_id)", m_dbConnection);
                        insertEntityComponentCommand.Parameters.Add(new SQLiteParameter("@entity_id", entityId));
                        insertEntityComponentCommand.Parameters.Add(new SQLiteParameter("@component_id", componentId));
                        insertEntityComponentCommand.Parameters.Add(new SQLiteParameter("@component_data_id", componentDataId));
                        insertEntityComponentCommand.ExecuteNonQuery();
                    }
                }
                else
                {
                    SQLiteCommand insertEntityComponentCommand = new SQLiteCommand("INSERT INTO _entity_components (entity_id, component_id) VALUES (@entity_id,@component_id)", m_dbConnection);
                    insertEntityComponentCommand.Parameters.Add(new SQLiteParameter("@entity_id", entityId));
                    insertEntityComponentCommand.Parameters.Add(new SQLiteParameter("@component_id", componentId));
                    insertEntityComponentCommand.ExecuteNonQuery();
                }

                /*
                if (componentSubsystemRegisterMap.ContainsKey(componentId))
                {
                    foreach (Func<Int64, bool> func in componentSubsystemRegisterMap[componentId])
                    {
                        func(entityId);
                    }
                }
                */
            }
        }

        private static Int64 AddComponentData(string componentTable, string parameters = null, params object[] values)
        {
            Int64 retComponentDataId = 0;
    
            SQLiteCommand insertComponentCommand = new SQLiteCommand();
            insertComponentCommand.Connection = m_dbConnection;
            if (parameters != null && values.Length > 0)
            {
                parameters.Replace(" ", String.Empty);
                string[] parametersSplit = parameters.Split(',');
                if (parametersSplit.Length == 0 || parametersSplit.Length != values.Length)
                {
                    return retComponentDataId;
                }

                string queryString = "INSERT INTO " + componentTable + " (" + parameters + ") VALUES (";
                for (int i = 1; i < values.Length; i++)
                {
                    queryString += "@param" + i + ",";
                }
                queryString += "@param" + values.Length + "); SELECT Last_Insert_Rowid();";

                insertComponentCommand.CommandText = queryString;

                for (int i = 1; i <= values.Length; i++)
                {
                    insertComponentCommand.Parameters.Add(new SQLiteParameter("@param" + i, values[i - 1]));
                }
            }
            else
            {
                insertComponentCommand.CommandText = "INSERT INTO " + componentTable + " DEFAULT VALUES; SELECT Last_Insert_Rowid();";
            }

            retComponentDataId = (Int64)insertComponentCommand.ExecuteScalar();

            return retComponentDataId;
        }

        public static void AddComponents(Int64 entityId, Int64 assemblageId)
        {
            SQLiteCommand getComponentsCommand = new SQLiteCommand("SELECT component_id from _assemblage_components WHERE assemblage_id = @assemblage_id", m_dbConnection);
            getComponentsCommand.Parameters.Add(new SQLiteParameter("@assemblage_id", assemblageId));
            SQLiteDataReader getComponentsReader = getComponentsCommand.ExecuteReader();
            while (getComponentsReader.Read())
            {
                Int64 componentId = (Int64)getComponentsReader["component_id"];
                AddComponent(entityId, componentId);
            }
        }

        public static void UpdateComponent(Int64 entityId, Int64 componentId, string parameters, params object[] values)
        {
            string componentTable = null;
            Int64 componentDataId = 0;

            SQLiteCommand getComponentTableCommand = new SQLiteCommand("SELECT table_name from _components WHERE component_id = @component_id", m_dbConnection);
            getComponentTableCommand.Parameters.Add(new SQLiteParameter("@component_id", componentId));
            SQLiteDataReader getComponentTableReader = getComponentTableCommand.ExecuteReader();
            if (getComponentTableReader.Read())
            {
                if (getComponentTableReader["table_name"] != DBNull.Value)
                {
                    componentTable = (string)getComponentTableReader["table_name"];
                }
            }

            SQLiteCommand getComponentDataIdCommand = new SQLiteCommand("SELECT component_data_id from _entity_components WHERE (entity_id = @entity_id AND component_id = @component_id)", m_dbConnection);
            getComponentDataIdCommand.Parameters.Add(new SQLiteParameter("@entity_id", entityId));
            getComponentDataIdCommand.Parameters.Add(new SQLiteParameter("@component_id", componentId));
            SQLiteDataReader getComponentDataIdReader = getComponentDataIdCommand.ExecuteReader();
            if (getComponentDataIdReader.Read())
            {
                if (getComponentDataIdReader["component_data_id"] != DBNull.Value)
                {
                    componentDataId = (Int64)getComponentDataIdReader["component_data_id"];
                }
            }

            if (componentTable != null && componentDataId > 0)
            {
                UpdateComponentData(componentTable, componentDataId, parameters, values);
            }
        }

        public static void UpdateComponent(Int64 entityId, string componentName, string parameters, params object[] values)
        {
            string componentTable = null;
            Int64 componentDataId = 0;
            Int64 componentId = 0;

            SQLiteCommand getComponentTableCommand = new SQLiteCommand("SELECT component_id, table_name from _components WHERE official_name = @component_name", m_dbConnection);
            getComponentTableCommand.Parameters.Add(new SQLiteParameter("@component_name", componentName));
            SQLiteDataReader getComponentTableReader = getComponentTableCommand.ExecuteReader();
            if (getComponentTableReader.Read())
            {
                componentId = (Int64)getComponentTableReader["component_id"];

                if (getComponentTableReader["table_name"] != DBNull.Value)
                {
                    componentTable = (string)getComponentTableReader["table_name"];
                }
            }

            SQLiteCommand getComponentDataIdCommand = new SQLiteCommand("SELECT component_data_id from _entity_components WHERE (entity_id = @entity_id AND component_id = @component_id)", m_dbConnection);
            getComponentDataIdCommand.Parameters.Add(new SQLiteParameter("@entity_id", entityId));
            getComponentDataIdCommand.Parameters.Add(new SQLiteParameter("@component_id", componentId));
            SQLiteDataReader getComponentDataIdReader = getComponentDataIdCommand.ExecuteReader();
            if (getComponentDataIdReader.Read())
            {
                if (getComponentDataIdReader["component_data_id"] != DBNull.Value)
                {
                    componentDataId = (Int64)getComponentDataIdReader["component_data_id"];
                }
            }

            if (componentTable != null && componentDataId > 0)
            {
                UpdateComponentData(componentTable, componentDataId, parameters, values);
            }
        }

        private static void UpdateComponentData(string componentTable, Int64 componentDataId, string parameters, params object[] values)
        {
            parameters.Replace(" ", String.Empty);
            string[] parametersSplit = parameters.Split(',');

            if (parametersSplit.Length > 0 && parametersSplit.Length == values.Length)
            {
                string queryString = "UPDATE " + componentTable + " SET ";
                for (int i = 1; i < parametersSplit.Length; i++)
                {
                    queryString += parametersSplit[i - 1] + "=@value" + i + ", ";
                }
                queryString += parametersSplit[parametersSplit.Length - 1] + "=@value" + parametersSplit.Length + " WHERE component_data_id=@component_data_id";

                SQLiteCommand updateComponentCommand = new SQLiteCommand(queryString, m_dbConnection);
                for (int i = 1; i <= values.Length; i++)
                {
                    updateComponentCommand.Parameters.Add(new SQLiteParameter("@value" + i, values[i - 1]));
                }
                updateComponentCommand.Parameters.Add(new SQLiteParameter("@component_data_id", componentDataId));
                updateComponentCommand.ExecuteNonQuery();
            }
        }

        public static void RemoveEntity(Int64 entityId)
        {
            RemoveAllComponents(entityId);

            SQLiteCommand deleteCommand = new SQLiteCommand("DELETE FROM _entities WHERE entity_id=@entity_id", m_dbConnection);
            deleteCommand.Parameters.Add(new SQLiteParameter("@entity_id", entityId));
            deleteCommand.ExecuteNonQuery();
        }

        public static void RemoveAllComponents(Int64 entityId)
        {
            SQLiteCommand getData = new SQLiteCommand("SELECT ec.component_id, table_name, component_data_id FROM _components AS c INNER JOIN _entity_components AS ec ON c.component_id=ec.component_id WHERE ec.entity_id=@entity_id", m_dbConnection);
            getData.Parameters.Add(new SQLiteParameter("@entity_id", entityId));
            SQLiteDataReader getDataReader = getData.ExecuteReader();
            while (getDataReader.Read())
            {
                if (getDataReader["component_data_id"] != DBNull.Value)
                {
                    string componentTable = (string)getDataReader["table_name"];
                    Int64 componentDataId = (Int64)getDataReader["component_data_id"];
                    RemoveComponentData(componentTable, componentDataId);
                }

                Int64 componentId = (Int64)getDataReader["component_id"];
                SQLiteCommand deleteCommand = new SQLiteCommand("DELETE FROM _entity_components WHERE entity_id=@entity_id AND component_id=@component_id", m_dbConnection);
                deleteCommand.Parameters.Add(new SQLiteParameter("@entity_id", entityId));
                deleteCommand.Parameters.Add(new SQLiteParameter("@component_id", componentId));
                deleteCommand.ExecuteNonQuery();

                /*
                if (componentSubsystemUnregisterMap.ContainsKey(componentId))
                {
                    foreach (Func<Int64, bool> func in componentSubsystemUnregisterMap[componentId])
                    {
                        func(entityId);
                    }
                }
                */
            }
        }

        public static void RemoveComponent(Int64 entityId, Int64 componentId)
        {
            SQLiteCommand getData = new SQLiteCommand("SELECT table_name, component_data_id FROM _components AS c INNER JOIN _entity_components AS ec ON c.component_id=ec.component_id WHERE (ec.entity_id=@entity_id AND ec.component_id=@component_id)", m_dbConnection);
            getData.Parameters.Add(new SQLiteParameter("@entity_id", entityId));
            getData.Parameters.Add(new SQLiteParameter("@component_id", componentId));
            SQLiteDataReader getDataReader = getData.ExecuteReader();
            if (getDataReader.Read())
            {
                if (getDataReader["component_data_id"] != DBNull.Value)
                {
                    string componentTable = (string)getDataReader["table_name"];
                    Int64 componentDataId = (Int64)getDataReader["component_data_id"];
                    RemoveComponentData(componentTable, componentDataId);
                }

                SQLiteCommand deleteCommand = new SQLiteCommand("DELETE FROM _entity_components WHERE entity_id=@entity_id AND component_id=@component_id", m_dbConnection);
                deleteCommand.Parameters.Add(new SQLiteParameter("@entity_id", entityId));
                deleteCommand.Parameters.Add(new SQLiteParameter("@component_id", componentId));
                deleteCommand.ExecuteNonQuery();

                /*
                if (componentSubsystemUnregisterMap.ContainsKey(componentId))
                {
                    foreach (Func<Int64, bool> func in componentSubsystemUnregisterMap[componentId])
                    {
                        func(entityId);
                    }
                }
                */
            }
        }

        public static void RemoveComponentData(string componentTable, Int64 componentDataId)
        {
            SQLiteCommand deleteCommand = new SQLiteCommand("DELETE FROM " + componentTable + " WHERE component_data_id=@component_data_id", m_dbConnection);
            deleteCommand.Parameters.Add(new SQLiteParameter("@component_data_id", componentDataId));
            deleteCommand.ExecuteNonQuery();
        }

        public static SQLiteCommand PrepareUpdateComponentCommand(string componentName, string parameters)
        {
            SQLiteCommand cmd = null;

            parameters.Replace(" ", String.Empty);
            string[] parametersSplit = parameters.Split(',');

            if (parametersSplit.Length > 0)
            {
                string componentTable = null;
                Int64 componentId = 0;

                SQLiteCommand getComponentTableCommand = new SQLiteCommand("SELECT component_id, table_name from _components WHERE official_name = @component_name", m_dbConnection);
                getComponentTableCommand.Parameters.Add(new SQLiteParameter("@component_name", componentName));
                using (SQLiteDataReader getComponentTableReader = getComponentTableCommand.ExecuteReader())
                {
                    if (getComponentTableReader.Read())
                    {
                        componentId = (Int64)getComponentTableReader["component_id"];

                        if (getComponentTableReader["table_name"] != DBNull.Value)
                        {
                            componentTable = (string)getComponentTableReader["table_name"];

                            string queryString = "UPDATE " + componentTable + " SET ";
                            for (int i = 1; i < parametersSplit.Length; i++)
                            {
                                queryString += parametersSplit[i - 1] + "=@param" + i + ", ";
                            }
                            queryString += parametersSplit[parametersSplit.Length - 1] + "=@param" + parametersSplit.Length
                                + " WHERE component_data_id=(SELECT component_data_id FROM _entity_components WHERE entity_id=@entity_id AND component_id=" + componentId + ")";

                            cmd = new SQLiteCommand(queryString, m_dbConnection);
                        }
                    }
                }
            }

            return cmd;
        }

        public static SQLiteCommand PrepareGetComponentDataCommand(params string[] componentNames)
        {
            SQLiteCommand getDataCommand = new SQLiteCommand(EntityManager.DBConnection);

            if (componentNames.Length > 0)
            {
                Dictionary<string, string> tableNameMap = new Dictionary<string, string>();
                SQLiteCommand getComponentTableCommand = new SQLiteCommand("SELECT table_name from _components WHERE official_name = @component_name", m_dbConnection);
                foreach (string componentName in componentNames)
                {
                    getComponentTableCommand.Parameters.Add(new SQLiteParameter("@component_name", componentName));
                    using (SQLiteDataReader getComponentTableReader = getComponentTableCommand.ExecuteReader())
                    {
                        if (getComponentTableReader.Read())
                        {
                            if (getComponentTableReader["table_name"] == DBNull.Value)
                            {
                                return getDataCommand; // quit
                            }
                            else
                            {
                                tableNameMap[componentName] = (string)getComponentTableReader["table_name"];
                            }
                        }
                    }
                }

                string sqlQuery = "SELECT * FROM _entity_components AS e INNER JOIN _components AS c ON e.component_id = c.component_id ";
                sqlQuery += "INNER JOIN '" + tableNameMap[componentNames[0]] + "' AS t ON (e.entity_id = @entity_id AND c.official_name = '"
                    + componentNames[0] + "' AND e.component_data_id = t.component_data_id) ";
                for (int j = 1; j < componentNames.Length; j++)
                {
                    sqlQuery += "INNER JOIN (SELECT * FROM _entity_components AS e INNER JOIN _components AS c ON e.component_id = c.component_id INNER JOIN '"
                        + tableNameMap[componentNames[j]] + "' AS t ON (e.entity_id = @entity_id AND c.official_name = '"
                        + componentNames[j] + "' AND e.component_data_id = t.component_data_id)) AS x" + j + " ON e.entity_id = x" + j + ".entity_id ";
                }

                getDataCommand.CommandText = sqlQuery;
            }

            return getDataCommand;
        }

        public static SQLiteCommand PrepareGetAllEntitiesWithComponents(params string[] componentNames)
        {
            SQLiteCommand cmd = new SQLiteCommand(EntityManager.DBConnection);

            if (componentNames.Length > 0)
            {
                string sqlQuery = "SELECT e.entity_id FROM _entity_components AS e INNER JOIN _components AS c ON e.component_id = c.component_id AND c.official_name = '" + componentNames[0] + "'";

                for (int j = 1; j < componentNames.Length; j++)
                {
                    sqlQuery += " INNER JOIN (SELECT e.entity_id FROM _entity_components AS e INNER JOIN _components AS c ON e.component_id = c.component_id  AND c.official_name = '"
                        + componentNames[j] + "') AS x" + j + " ON e.entity_id = x" + j + ".entity_id";
                }

                cmd.CommandText = sqlQuery;
            }

            return cmd;
        }

        /*
        public static SQLiteCommand PrepareGetComponentData(string componentName)
        {
            SQLiteCommand cmd = new SQLiteCommand(EntityManager.DBConnection);

            SQLiteCommand getComponentTableCommand = new SQLiteCommand("SELECT table_name from _components WHERE official_name = @component_name", m_dbConnection);
            getComponentTableCommand.Parameters.Add(new SQLiteParameter("@component_name", componentName));
            SQLiteDataReader getComponentTableReader = getComponentTableCommand.ExecuteReader();
            if (getComponentTableReader.Read())
            {
                if (getComponentTableReader["table_name"] != DBNull.Value)
                {
                    string componentTable = (string)getComponentTableReader["table_name"];

                    string sqlQuery = "SELECT * FROM '" + componentTable + "' WHERE component_data_id = (SELECT component_data_id FROM _entity_components WHERE entity_id = @entity_id)";
                    cmd.CommandText = sqlQuery;
                }
            }        

            return cmd;
        }
        */
    }
}

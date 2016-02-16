using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using Microsoft.Xna.Framework;

namespace WinShooterGame.Core
{
    class Subsystem
    {
        /*
        protected Int64[] subscribedComponentIds;
        protected ISet<Int64> registeredEntityIds = new HashSet<Int64>();

        public Subsystem(params string[] subscribedComponentNames)
        {
            if (subscribedComponentNames != null && subscribedComponentNames.Length > 0)
            {
                string sqlQuery = "SELECT component_id,official_name,table_name FROM _components WHERE official_name IN (";
                for (int i = 1; i < subscribedComponentNames.Length; i++)
                {
                    sqlQuery += "@name" + i + ",";
                }
                sqlQuery += "@name" + subscribedComponentNames.Length + ")";
                SQLiteCommand getComponentIds = new SQLiteCommand(sqlQuery, EntityManager.DBConnection);
                for (int i = 1; i <= subscribedComponentNames.Length; i++)
                {
                    getComponentIds.Parameters.Add(new SQLiteParameter("@name" + i, subscribedComponentNames[i - 1]));
                }

                SQLiteDataReader getComponentIdsReader = getComponentIds.ExecuteReader();
                subscribedComponentIds = new Int64[subscribedComponentNames.Length];
                while (getComponentIdsReader.Read())
                {
                    subscribedComponentIds[getComponentIdsReader.StepCount - 1] = (Int64)getComponentIdsReader["component_id"];
                    //EntityManager.ComponentNameIdMap[(string)getComponentIdsReader["official_name"]] = (Int64)getComponentIdsReader["component_id"];
                    if (getComponentIdsReader["table_name"] != DBNull.Value)
                    {
                        EntityManager.ComponentNameTableNameMap[(string)getComponentIdsReader["official_name"]] = (string)getComponentIdsReader["table_name"]; 
                    }
                    else
                    {
                        EntityManager.ComponentNameTableNameMap[(string)getComponentIdsReader["official_name"]] = null;
                    }
                }
            }

            if (subscribedComponentIds != null)
            {

                foreach (Int64 componentId in subscribedComponentIds)
                {
                    if (EntityManager.ComponentSubsystemRegisterMap.ContainsKey(componentId) == false)
                    {
                        EntityManager.ComponentSubsystemRegisterMap.Add(componentId, new List<Func<Int64, bool>>());
                    }
                    EntityManager.ComponentSubsystemRegisterMap[componentId].Add(this.RegisterEntity);

                    if (EntityManager.ComponentSubsystemUnregisterMap.ContainsKey(componentId) == false)
                    {
                        EntityManager.ComponentSubsystemUnregisterMap.Add(componentId, new List<Func<Int64, bool>>());
                    }
                    EntityManager.ComponentSubsystemUnregisterMap[componentId].Add(this.UnregisterEntity);
                }
            }
        }

        public bool RegisterEntity(Int64 entityId)
        {
            bool result = false;

            if (subscribedComponentIds != null && subscribedComponentIds.Length > 0 && registeredEntityIds.Contains(entityId) == false)
            {
                SQLiteCommand getCount = new SQLiteCommand("SELECT COUNT(*) FROM _entity_components WHERE entity_id=@entity_id", EntityManager.DBConnection);
                getCount.Parameters.Add(new SQLiteParameter("@entity_id", entityId));
                Int64 count = (Int64)getCount.ExecuteScalar();

                if (count >= subscribedComponentIds.Length)
                {
                    ISet<Int64> componentIds = new HashSet<Int64>();

                    SQLiteCommand getComponentIds = new SQLiteCommand("SELECT component_id FROM _entity_components WHERE entity_id=@entity_id", EntityManager.DBConnection);
                    getComponentIds.Parameters.Add(new SQLiteParameter("@entity_id", entityId));
                    SQLiteDataReader getComponentIdsReader = getComponentIds.ExecuteReader();
                    while (getComponentIdsReader.Read())
                    {
                        componentIds.Add((Int64)getComponentIdsReader["component_id"]);
                    }

                    bool containsAllComponents = true;
                    foreach (Int64 subscribedComponentId in subscribedComponentIds)
                    {
                        if (componentIds.Contains(subscribedComponentId) == false)
                        {
                            containsAllComponents = false;
                            break;
                        }
                    }
                    if (containsAllComponents)
                    {
                        registeredEntityIds.Add(entityId);
                        result = true;
                    }
                }
            }

            return result;
        }

        public bool UnregisterEntity(Int64 entityId)
        {
            bool result = false;

            if (subscribedComponentIds != null && subscribedComponentIds.Length > 0 && registeredEntityIds.Contains(entityId) == true)
            {
                registeredEntityIds.Remove(entityId);
            }

            return result;
        }

        protected SQLiteDataReader GetComponentDataOfRegisteredEntities(params string[] componentNames)
        {
            if (registeredEntityIds.Count < 1 || componentNames.Length < 1) {
                return null;
            }
         
            SQLiteCommand getDataCommand = new SQLiteCommand(EntityManager.DBConnection);

            string sqlQuery = "SELECT * FROM _entity_components AS e INNER JOIN _components AS c ON e.component_id = c.component_id ";
            sqlQuery += "INNER JOIN " + EntityManager.ComponentNameTableNameMap[componentNames[0]] + " AS t ON (e.entity_id IN (";
            for (int i = 1; i < registeredEntityIds.Count; i++)
            {
                sqlQuery += "@entity_id" + i + ",";
            }
            sqlQuery += "@entity_id" + registeredEntityIds.Count + ") AND c.official_name=@component_name1 AND e.component_data_id = t.component_data_id) ";

            for (int j = 1; j < componentNames.Length; j++)
            {
                sqlQuery += "INNER JOIN (SELECT * FROM _entity_components AS e INNER JOIN _components AS c ON e.component_id = c.component_id INNER JOIN "
                    + EntityManager.ComponentNameTableNameMap[componentNames[j]] + " AS t ON (e.entity_id IN (";
                for (int i = 1; i < registeredEntityIds.Count; i++)
                {
                    sqlQuery += "@entity_id" + i + ",";
                }
                sqlQuery += "@entity_id" + registeredEntityIds.Count + ") AND c.official_name=@component_name" 
                    + (j+1) + " AND e.component_data_id = t.component_data_id)) AS x" + j + " ON e.entity_id = x" + j + ".entity_id ";
            }

            int k = 1;
            foreach (string componentName in componentNames)
            {
                getDataCommand.Parameters.Add(new SQLiteParameter("@component_name" + k, componentName));
                k++;
            }
            k = 1;
            foreach (Int64 entityId in registeredEntityIds)
            {
                getDataCommand.Parameters.Add(new SQLiteParameter("@entity_id" + k, entityId));
                k++;
            }

            getDataCommand.CommandText = sqlQuery;
            SQLiteDataReader getDataReader = getDataCommand.ExecuteReader();

            return getDataReader;
        }
        */

        public virtual void Update(GameTime gameTime) { }
    }
}

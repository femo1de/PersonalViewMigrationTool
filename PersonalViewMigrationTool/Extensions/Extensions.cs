using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace PersonalViewMigrationTool.Extensions
{
    public static class Extensions
    {
        internal static void AddOrReplace<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, TValue value)
        {
            if (dic.ContainsKey(key))
            {
                // replace this value
                dic[key] = value;
            }
            else
            {
                // add this value 
                dic.Add(key, value);
            }
        }

        public static Entity Copy(this Entity record, params string[] columns)
        {
            var result = new Entity(record.LogicalName, record.Id);

            var columnsToCopy = columns;

            if (columnsToCopy.Length == 0)
            {
                columnsToCopy = record.Attributes.Keys.ToArray();
            }

            foreach (var column in columnsToCopy)
            {
                if (record.Contains(column))
                    result[column] = record[column];
            }

            return result;
        }

        public static Guid Upsert(this IOrganizationService service, Entity record)
        {
            var query = new QueryByAttribute(record.LogicalName)
            {
                ColumnSet = new ColumnSet(false)
            };
            query.AddAttributeValue($"{record.LogicalName}id", record.Id);

            var existingRecord = service.RetrieveMultiple(query).Entities.FirstOrDefault();

            if (existingRecord == null)
            {
                return service.Create(record);
            }

            service.Update(record);
            return record.Id;
        }

        private static string CreateFetchXml(string initialFetchXml, string pagingCookie, int pageNumber, int fetchCount)
        {
            var doc = new XmlDocument();
            doc.LoadXml(initialFetchXml);

            if (doc.DocumentElement == null)
                throw new InvalidPluginExecutionException("Document element of Xml is empty!");

            var attrs = doc.DocumentElement.Attributes;

            if (!string.IsNullOrEmpty(pagingCookie))
            {
                var pagingAttr = doc.CreateAttribute("paging-cookie");
                pagingAttr.Value = pagingCookie;
                attrs.Append(pagingAttr);
            }

            var pageAttr = doc.CreateAttribute("page");
            pageAttr.Value = Convert.ToString(pageNumber);
            attrs.Append(pageAttr);

            var countAttr = doc.CreateAttribute("count");
            countAttr.Value = Convert.ToString(fetchCount);
            attrs.Append(countAttr);

            return doc.OuterXml;
        }


        public static List<Entity> RetrieveAll(this IOrganizationService service, QueryBase query)
        {
            var results = new List<Entity>();
            var initialFetchXml = string.Empty;
            var pageNumber = 1;

            switch (query)
            {
                case QueryByAttribute _:
                    ((QueryByAttribute)query).PageInfo = new PagingInfo()
                    {
                        Count = 500,
                        PageNumber = 1
                    };
                    break;
                case QueryExpression _:
                    ((QueryExpression)query).PageInfo = new PagingInfo()
                    {
                        Count = 500,
                        PageNumber = 1
                    };
                    break;
                case FetchExpression _:
                    initialFetchXml = ((FetchExpression)query).Query;
                    var fetchXml = CreateFetchXml(initialFetchXml, null, 1, 500);
                    ((FetchExpression)query).Query = fetchXml;
                    break;
                default:
                    throw new Exception($"Paging for {query.GetType().FullName} is not supported yet!");
            }

            EntityCollection records;

            do
            {
                records = service.RetrieveMultiple(query);

                results.AddRange(records.Entities);

                switch (query)
                {
                    case QueryByAttribute _:
                        ((QueryByAttribute)query).PageInfo.PageNumber++;
                        ((QueryByAttribute)query).PageInfo.PagingCookie = records.PagingCookie;
                        break;
                    case QueryExpression _:
                        ((QueryExpression)query).PageInfo.PageNumber++;
                        ((QueryExpression)query).PageInfo.PagingCookie = records.PagingCookie;
                        break;
                    default:
                        pageNumber++;
                        var fetchXml = CreateFetchXml(initialFetchXml, records.PagingCookie, pageNumber, 500);
                        ((FetchExpression)query).Query = fetchXml;
                        break;
                }
            } while (records.MoreRecords);

            return results;
        }
    }
}

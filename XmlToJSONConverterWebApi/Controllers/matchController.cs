using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Xml;
using XmlToJSONConverterWebApi.Entities;
using XmlToJSONConverterWebApi.Models;
using Qualifier = XmlToJSONConverterWebApi.Entities.Qualifier;

namespace XmlToJSONConverterWebApi.Controllers
{
    public class matchController : ApiController
    {


        /// <summary>
        /// Gets the information about match with "id" in JSON format.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.Web.Http.HttpResponseException"></exception>
        // GET api/match/5
        public object Get(long id)
        {
            List<FactEvent> games;
            using (var context = new FactsEntities())
            {
                games = context.FactEvents.Where(i => i.fixture_id == id).Include("Qualifiers").ToList();
            }
            if (games == null || games.Count == 0)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent(string.Format("No match with ID = {0}", id)),
                    ReasonPhrase = "match ID Not Found"
                };
                throw new HttpResponseException(resp);
            }
            var match = new MatchModel
            {
                fixtureId = id,
                Events = new List<object>()
            };

            foreach (var factEvent in games)
            {
                var qualifiers = new List<object>();
                foreach (var qualifier in factEvent.Qualifiers)
                {
                    var dict = new Dictionary<string, object>();
                    var eventQualifier = new ExpandoObject();
                    dict.Add("Id", qualifier.ID);
                    dict.Add("name", qualifier.name);
                    dict.Add("qualifierId", qualifier.qualifier_id);
                    dict.Add("value", qualifier.value);
                    ProcessAdditional(qualifier.qualifierAdditional, dict);

                    var eventQualifierColl = (ICollection<KeyValuePair<string, object>>)eventQualifier;
                    foreach (var kvp in dict)
                    {
                        eventQualifierColl.Add(kvp);
                    }

                    qualifiers.Add(eventQualifier);
                }
                var gameDictionary = new Dictionary<string, object>();
                var game = new ExpandoObject();
                gameDictionary.Add("awayScore", factEvent.away_score);
                gameDictionary.Add("gameId", factEvent.fixture_id);
                gameDictionary.Add("homeScore", factEvent.home_score);
                gameDictionary.Add("status", factEvent.game_status);
                gameDictionary.Add("timerRunning", factEvent.timer_running);
                ProcessAdditional(factEvent.gameAdditional, gameDictionary);

                var gameColl = (ICollection<KeyValuePair<string, object>>)game;
                foreach (var kvp in gameDictionary)
                {
                    gameColl.Add(kvp);
                }

                var eventDictionary = new Dictionary<string, object>();
                var fEvent = new ExpandoObject();
                eventDictionary.Add("id", factEvent.opta_event_id);
                eventDictionary.Add("direction", factEvent.direction);
                eventDictionary.Add("directionOfPlayX", factEvent.direction_of_play_x);
                eventDictionary.Add("directionOfPlayY", factEvent.direction_of_play_y);
                eventDictionary.Add("eventId", factEvent.event_id);
                eventDictionary.Add("eventTypeId", factEvent.event_type_id);
                eventDictionary.Add("eventTypeName", factEvent.event_type_name);
                eventDictionary.Add("lastModified", factEvent.last_modified);
                eventDictionary.Add("outcome", factEvent.outcome);
                eventDictionary.Add("period", factEvent.period);
                eventDictionary.Add("periodId", factEvent.period_id);
                eventDictionary.Add("periodMinute", factEvent.period_minute);
                eventDictionary.Add("periodSecond", factEvent.period_second);
                eventDictionary.Add("playerId", factEvent.player_id);
                eventDictionary.Add("teamId", factEvent.team_id);
                eventDictionary.Add("timestamp", factEvent.timestamp);
                eventDictionary.Add("timestampMiliseconds", factEvent.timestamp_milliseconds);
                eventDictionary.Add("x", factEvent.x);
                eventDictionary.Add("y", factEvent.y);
                eventDictionary.Add("qualifiers", qualifiers);
                ProcessAdditional(factEvent.eventAdditional, eventDictionary);

                var eventColl = (ICollection<KeyValuePair<string, object>>)fEvent;
                foreach (var kvp in eventDictionary)
                {
                    eventColl.Add(kvp);
                }

                var record = new
                {
                    Game = game,
                    Event = fEvent
                };
                match.Events.Add(record);
            }

            return match;
        }

        /// <summary>
        /// Receives XML file and store this data in database
        /// </summary>
        /// <exception cref="System.Exception">Can't parse xml. Probably this is not valid xml file!</exception>
        [Route("api/match/sendXML")]
        [AcceptVerbs("GET", "POST")]
        public void SendXML()
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                var httpRequest = HttpContext.Current.Request;
                //foreach (string file in httpRequest.Files)
                for (int i = 0; i < httpRequest.Files.Count; i++)
                {
                    var postedFile = httpRequest.Files[i];
                    //if (!postedFile.ContentType.Contains("xml"))
                    //    throw new Exception("Not valid xml file!");
                    doc.Load(postedFile.InputStream);
                    ProcessFile(doc);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Can't parse xml. Probably this is not valid xml file!", ex);
            }
        }

        /// <summary>
        /// Processes the file.
        /// </summary>
        /// <param name="doc">The document.</param>
        private void ProcessFile(XmlDocument doc)
        {
            var entity = new FactEvent();
            try
            {
                using (var context = new FactsEntities())
                {
                    ProcessNodes(doc, entity, context);

                    context.FactEvents.Add(entity);
                    context.SaveChanges();
                }
            }
            catch (DbUpdateException)
            {

            }
        }

        /// <summary>
        /// Processes the nodes.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="factEvent">The fact event.</param>
        /// <param name="context">The context.</param>
        private void ProcessNodes(XmlNode parent, FactEvent factEvent, FactsEntities context)
        {
            if (parent.Attributes != null && parent.Attributes.Count > 0)
            {
                ProcessAttributes(parent, factEvent, context);
            }

            if (parent.HasChildNodes)
            {
                foreach (XmlNode node in parent.ChildNodes)
                {
                    ProcessNodes(node, factEvent, context);
                }
            }
        }

        /// <summary>
        /// Processes the attributes.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="factEvent">The fact event.</param>
        /// <param name="context">The context.</param>
        private void ProcessAttributes(XmlNode parent, FactEvent factEvent, FactsEntities context)
        {
            if (parent.Name != "Qualifiers")
            {
                foreach (XmlAttribute attribute in parent.Attributes)
                {

                    switch (attribute.Name)
                    {
                        case "away_score":
                            factEvent.away_score = short.Parse(attribute.Value);
                            break;
                        case "home_score":
                            factEvent.home_score = short.Parse(attribute.Value);
                            break;
                        case "game_id":
                            factEvent.fixture_id = long.Parse(attribute.Value);
                            break;
                        case "status":
                            factEvent.game_status = attribute.Value;
                            break;
                        case "timer_running":
                            factEvent.timer_running = byte.Parse(attribute.Value);
                            break;
                        case "id":
                            factEvent.opta_event_id = long.Parse(attribute.Value);
                            break;
                        case "direction":
                            factEvent.direction = attribute.Value;
                            break;
                        case "direction_of_play_x":
                            factEvent.direction_of_play_x = decimal.Parse(attribute.Value, CultureInfo.InvariantCulture);
                            break;
                        case "direction_of_play_y":
                            factEvent.direction_of_play_y = decimal.Parse(attribute.Value, CultureInfo.InvariantCulture);
                            break;
                        case "event_id":
                            factEvent.event_id = short.Parse(attribute.Value);
                            break;
                        case "event_type_id":
                            factEvent.event_type_id = short.Parse(attribute.Value);
                            break;
                        case "event_type_name":
                            factEvent.event_type_name = attribute.Value;
                            break;
                        case "last_modified":
                            factEvent.last_modified = DateTime.Parse(attribute.Value);
                            break;
                        case "outcome":
                            factEvent.outcome = short.Parse(attribute.Value);
                            break;
                        case "period":
                            factEvent.period = attribute.Value;
                            break;
                        case "period_id":
                            factEvent.period_id = short.Parse(attribute.Value);
                            break;
                        case "period_minute":
                            factEvent.period_minute = byte.Parse(attribute.Value);
                            break;
                        case "period_second":
                            factEvent.period_second = byte.Parse(attribute.Value);
                            break;
                        case "player_id":
                            factEvent.player_id = long.Parse(attribute.Value);
                            break;
                        case "team_id":
                            factEvent.team_id = long.Parse(attribute.Value);
                            break;
                        case "timestamp":
                            factEvent.timestamp = DateTime.Parse(attribute.Value);
                            break;
                        case "timestamp_milliseconds":
                            factEvent.timestamp_milliseconds = attribute.Value;
                            break;
                        case "x":
                            factEvent.x = decimal.Parse(attribute.Value, CultureInfo.InvariantCulture);
                            break;
                        case "y":
                            factEvent.y = decimal.Parse(attribute.Value, CultureInfo.InvariantCulture);
                            break;
                        default:
                            if (parent.Name.ToLower() == "game")
                            {
                                factEvent.gameAdditional +=
                                    string.Format("<attribute><name>{0}</name><value>{1}</value></attribute>",
                                        attribute.Name, attribute.Value);
                            }
                            else
                            {
                                factEvent.eventAdditional +=
                                    string.Format("<attribute><name>{0}</name><value>{1}</value></attribute>",
                                        attribute.Name, attribute.Value);
                            }
                            break;
                    }

                }

            }
            if (parent.Name == "Qualifiers")
            {
                var qualifier = new Qualifier();
                foreach (XmlAttribute qualifierAttribute in parent.Attributes)
                {
                    switch (qualifierAttribute.Name)
                    {
                        case "id":
                            qualifier.ID = long.Parse(qualifierAttribute.Value);
                            break;
                        case "name":
                            qualifier.name = qualifierAttribute.Value;
                            break;
                        case "qualifier_id":
                            qualifier.qualifier_id = long.Parse(qualifierAttribute.Value);
                            break;
                        case "value":
                            qualifier.value = qualifierAttribute.Value;
                            break;
                        default:
                            qualifier.qualifierAdditional +=
                                    string.Format("<attribute><name>{0}</name><value>{1}</value></attribute>",
                                        qualifierAttribute.Name, qualifierAttribute.Value);
                            break;
                    }
                }
                context.Qualifiers.Add(qualifier);
                factEvent.Qualifiers.Add(qualifier);
            }
            factEvent.created = DateTime.Now;
        }

        /// <summary>
        /// Processes the additional information.
        /// </summary>
        /// <param name="additional">The additional.</param>
        /// <param name="dict">The dictionary.</param>
        private void ProcessAdditional(string additional, Dictionary<string, object> dict)
        {
            if (additional == null)
                return;
            var attributes = additional.Split(new[] { "<attribute>", "</attribute>" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var attribute in attributes)
            {
                var nameStart = attribute.IndexOf("<name>", StringComparison.Ordinal) + "<name>".Length;
                var name = attribute.Substring(nameStart, attribute.IndexOf("</name>", StringComparison.Ordinal) - nameStart);
                var valueStart = attribute.IndexOf("<value>", StringComparison.Ordinal) + "<value>".Length;
                var value = attribute.Substring(valueStart, attribute.IndexOf("</value>", StringComparison.Ordinal) - valueStart);
                dict.Add(name, value);
            }
        }
    }
}

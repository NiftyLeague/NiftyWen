using CodeStage.AntiCheat.ObscuredTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class EventController : MonoBehaviour
{
	private static EventController I;
	private static string url = "https://odgwhiwhzb.execute-api.us-east-1.amazonaws.com/prod/matches/wen-game";

	public float updateInterval;

	private static Match match;

	private void Awake()
	{
		if (I != null && I != this && I.gameObject != null)
		{
			DestroyImmediate(gameObject);
			return;
		}
		I = this;
		if (I)
		{
			DontDestroyOnLoad(gameObject);
		}

		StartCoroutine(UpdateEvents());
	}


	IEnumerator UpdateEvents()
	{
		while (true)
		{
			yield return new WaitForSecondsRealtime(updateInterval);
			yield return UploadEvents();
		}
	}


	private static IEnumerator UploadEvents()
	{
		if (match == null || string.IsNullOrEmpty(match.id))
		{
			yield break;
		}
		string token = NiftyUsers.GetMyAuthorization();
		UnityWebRequest www = null;
		Dictionary<string, string> headers = new Dictionary<string, string>
		{
			{ "authorizationToken", token },
			{ "NL-Encryption", "1.1" },
		};
		string body = JsonUtility.ToJson(match);

		CleanUpMatch();
		yield return Utils.PostRequest(url, Encoding.ASCII.GetBytes(XUtils.ToXorBase64(body, token)), (w) => www = w, headers);
		if (www.result != UnityWebRequest.Result.Success)
		{
			Debug.Log(www.error);
			yield break;
		}
		else
		{
			//print(www.downloadHandler.text);
		}
	}

	public static bool IsMatchInProgress()
	{
		return match != null;
	}

	public static string AddMatchStart(string matchId)
	{
		try
		{
			match = new Match(matchId);
			AddEvent(new Event(EventTypes.MatchStart, null));
			print($"{match.shortId} - {match.id}");
		}
		catch (Exception e)
		{
			Debug.LogWarning($"Failed to add ({EventTypes.MatchStart}) event {e}");
		}
		return matchId;
	}

	public static void AddMatchEnd(Player winner, List<Player> players)
	{
		try
		{
			AddEvent(new Event(EventTypes.MatchEnd, null));
		}
		catch (Exception e)
		{
			Debug.LogWarning($"Failed to add ({EventTypes.MatchEnd}) event {e}");
		}
	}


	private static void CleanUpMatch()
	{
		if (match.events.Any(e => e.type == EventTypes.MatchEnd))
		{
			match.ClearEvents();
			match = null;
		}
		else
		{
			match.ClearEvents();
		}
	}

	private static void AddEvent(Event e)
	{
		if (match != null)
		{
			match.AddEvent(e);

			if (e.type == EventTypes.MatchEnd)
			{
				I.StartCoroutine(UploadEvents());
			}
		}
		else
		{
			print($"No match to add event '{e.type}'");
		}
	}

	[Serializable]
	private class Match
	{
		public string id;
		public string shortId;
		public List<Event> events;

		public Match(string id)
		{
			this.id = id;
			shortId = XUtils.GetInt32HashCode(id).ToString("X6");
			events = new List<Event>();
		}

		public void AddEvent(Event e)
		{
			events.Add(e);
		}

		public void ClearEvents()
		{
			events.Clear();
		}
	}


	[Serializable]
	private class Event
	{
		public string id;
		public ObscuredString type;
		public int ts;
		public string value;

		public Event(string type, string value)
		{
			// print($"{type}-{id}-{Mathf.FloorToInt(PhotonNetwork.ServerTimestamp / 100000)}");
			ts = (int)XUtils.Timestamp();
			this.id = GenerateIdHash($"{type}-{ts}");
			this.type = type;
			this.value = value;
		}

		private static string GenerateIdHash(string e)
		{
			return Utils.GetMD5Hash(e).Substring(0, 6);
		}
	}

	private static class EventTypes
	{
		public static ObscuredString MatchStart = "match-start";
		public static ObscuredString MatchEnd = "match-end";
		public static ObscuredString Score = "score";
		public static ObscuredString DodgeBall = "dodge-ball";
		public static ObscuredString DodgeBomb = "dodge-bomb";
	}
}

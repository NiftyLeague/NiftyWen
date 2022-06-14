using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using CodeStage.AntiCheat.ObscuredTypes;
using Newtonsoft.Json.Linq;

public class GameplayManager : MonoBehaviour
{
	public static GameplayManager I;

	public MenuManager menuManager;
	public AudioManager audioManager;
	public WenManager wenManager;
	public Character playerCharacter;
	[Space]
	public ObscuredInt score;
	public ObscuredFloat timePlayed;
	public ObscuredInt totalBalls;
	public ObscuredInt hits;
	public ObscuredInt misses;
	public ObscuredInt dodges;
	public ObscuredInt machineHits;
	public ObscuredInt xp;
	[Space]
	public ObscuredFloat currentSpeedIncrease;
	public CameraShake cameraShake;
	[Space]
	public TextMeshProUGUI scoreText;
	public TextMeshProUGUI wenMessageText;
	public TextMeshProUGUI soonMessageText;
	public TextMeshProUGUI scoreGainedText;
	public TextMeshProUGUI gameOverStatNamesText;
	public TextMeshProUGUI gameOverStatNumbersText;
	[Space]
	public SimpleAnim ballMachineAnim;
	public Sprite[] ballMachineSpritesIdle;
	public Sprite[] ballMachineSpritesFire;
	public Sprite[] ballMachineSpritesHitFlash;
	public CameraShake ballMachineShaker;
	public GameObject ballMachinePlayerKiller;
	[Space]
	public GameObject ballPrefab;
	public GameObject bombPrefab;
	public GameObject wenLineRendererObject;
	public Transform ballStartLocation;
	public Transform worldLeftLimit;
	public Transform worldRightLimit;
	[Space]
	public List<Color32> randomScoreGainedColors;

	public ObscuredFloat maxBallSpeed;
	public Vector2 wenSoonTimeRange;
	public Vector2 startTimeoutRange;
	public Vector2 shootRandomTimeoutRange;
	public TweenEaseType textTweenType;
	public ObscuredFloat textTweenDuration;
	public Vector2 textDisplayTimeRange;
	public Vector2 bombSpawnProbabilityRange;
	public ObscuredInt minScoreForBomb;
	public ObscuredBool hasGameEnded;

	ObscuredBool hasLaunchedABomb;
	ObscuredInt bombsFiredInARow;
	ObscuredBool pitching = false;

	private InputState input = new InputState();
	private Projectile currentProjectile = null;

	private void Awake()
	{
		I = this;
	}

	void Start()
	{
		UpdateScoreText();
		ResetEverythingForANewGame();
		ResetWenMessageTexts();
		menuManager.menuPanel.SetActive(false);
	}

	private void FixedUpdate()
	{
		if (hasGameEnded)
		{
			return;
		}

		timePlayed += Time.deltaTime;
		if (currentProjectile == null && !pitching)
		{
			StartCoroutine(PitchNextProjectile());
		}
	}

	public void ScorePoint(bool hitBallMachine)
	{
		int scoreGainedAmount = 1;
		if (hitBallMachine)
		{
			scoreGainedAmount = 2;
		}
		score += scoreGainedAmount;
		hits++;
		scoreGainedText.text = "+" + scoreGainedAmount.ToString("0");
		StartCoroutine(AnimateScoreText());
		IncreaseSpeed();
		UpdateScoreText();
		EventController.AddScore(scoreGainedAmount);
	}

	void UpdateScoreText()
	{
		scoreText.text = score.ToString("0");
	}

	public void Lose()
	{
		if (hasGameEnded)
		{
			return;
		}
		hasGameEnded = true;
		cameraShake.Shake(0.5f, 5);
		IncreaseSpeed(true);
		playerCharacter.Lose();
		menuManager.UpdateLeaderboards();
		EventController.AddMatchEnd(PlayerSpriteManager.lastDegenIdUsed);

		StartCoroutine(PlayGameOverScreen());

		Analytics.SendPlayerEvent("EndMatch", new Dictionary<string, string>() { { "Score", score.ToString() } });

	}

	public void Explosion(Vector3 position)
	{
		EffectsController.CreateExplosion(position);
		audioManager.PlaySound(AudioManager.SoundID.explosion);
	}

	public void HitBallMachine()
	{
		ballMachineShaker.Shake(0.2f, 10);
		audioManager.PlaySound(AudioManager.SoundID.ballMachineHit);
		StartCoroutine(BallMachineHitFlash());
	}

	IEnumerator BallMachineHitFlash()
	{
		ballMachineAnim.Play(ballMachineSpritesHitFlash, false);

		yield return new WaitForSeconds(0.2f);

		ballMachineAnim.Play(ballMachineSpritesIdle, false);
	}

	public void Dodge(bool isABomb)
	{
		if (hasGameEnded)
		{
			return;
		}

		if (isABomb)
		{
			dodges++;
			EventController.AddDodgeBomb();
		}
		else
		{
			misses++;
			EventController.AddDodgeBall();
		}
		IncreaseSpeed();
	}

	public void IncreaseSpeed(bool reset = false)
	{
		currentSpeedIncrease += 0.5f;
		if (reset)
		{
			currentSpeedIncrease = 0;
		}
	}

	public void ResetEverythingForANewGame()
	{
		hits = 0;
		misses = 0;
		dodges = 0;
		score = 0;
		machineHits = 0;
		xp = 0;
		timePlayed = 0;

		gameOverStatNamesText.text = "";
		gameOverStatNumbersText.text = "";
		scoreGainedText.transform.localScale = new Vector3(0, 0, 0);

		menuManager.ResetLeaderboardDisplay();

		ballMachinePlayerKiller.gameObject.SetActive(false);

		bombsFiredInARow = 0;
		hasGameEnded = false;
		hasLaunchedABomb = false;

		playerCharacter.UnLose();
		currentProjectile = null;

		UpdateScoreText();
		Analytics.SendPlayerEvent("StartMatch");
	}

	void ResetWenMessageTexts()
	{
		wenMessageText.transform.localScale = Vector3.zero;
		soonMessageText.transform.localScale = Vector3.zero;
	}

	IEnumerator PlayGameOverScreen()
	{
		audioManager.PlaySound(AudioManager.SoundID.lose);

		scoreText.text = "GAME OVER";

		yield return new WaitForSeconds(3f);
		yield return GetMatchResults();

		playerCharacter.StandBackUp();

		scoreText.text = "";

		float secondsPlayed = timePlayed % 60;
		float minutesPlayed = timePlayed / 60;
		float hoursPlayed = timePlayed / 60 / 60;

		string statNames = "SCORE\nTIME PLAYED\nTOTAL BALLs\nHITS\nMISSES";
		string statValeues = score.ToString("0") + "\n" + hoursPlayed.ToString("0") + ":" + minutesPlayed.ToString("00") + ":" + secondsPlayed.ToString("00") + "\n";
		statValeues += totalBalls.ToString("0") + "\n" + hits.ToString("0") + "\n" + misses.ToString("0");

		if (machineHits > 0)
		{
			statNames += "\nMachine Hits";
			statValeues += "\n" + machineHits.ToString("0");
		}

		if (hasLaunchedABomb)
		{
			statNames += "\nBOMBS DODGED";
			statValeues += "\n" + dodges.ToString("0");
		}

		if (xp > 0)
		{
			statNames += "\nXP";
			statValeues += "\n+" + xp.ToString("0");
		}

		gameOverStatNamesText.text = statNames.ToUpper();
		gameOverStatNumbersText.text = statValeues.ToUpper();
		yield return new WaitForSeconds(3);

		gameOverStatNamesText.text = "";
		gameOverStatNumbersText.text = "";

		menuManager.leaderboardType = 0;
		menuManager.UpdateLeaderboardDisplay();

		menuManager.SetMenuEnabled(true);
	}

	private IEnumerator GetMatchResults()
	{
		print(EventController.GetLastestMatchId());
		string result = null;
		yield return WebRequestHelper.GetRequest("https://odgwhiwhzb.execute-api.us-east-1.amazonaws.com/prod/matches/wen-game/results",
			$"id={EventController.GetLastestMatchId()}", true, false, resp => result = resp);
		try
		{
			JObject stats = JObject.Parse(result);
			int hits = stats["hits"] != null ? (int)stats["hits"] : 0;
			int misses = stats["misses"] != null ? (int)stats["misses"] : 0;
			int dodges = stats["dodges"] != null ? (int)stats["dodges"] : 0;
			int score = stats["score"] != null ? (int)stats["score"] : 0;
			int machineHits = stats["machine_hits"] != null ? (int)stats["machine_hits"] : 0;
			int xp = stats["xp"] != null ? (int)stats["xp"] : 0;
			int timePlayed = stats["time_played"] != null ? (int)stats["time_played"] : 0;

			this.hits = hits;
			this.misses = misses;
			this.dodges = dodges;
			this.score = score;
			this.machineHits = machineHits;
			this.xp = xp;
			this.timePlayed = timePlayed;


		}
		catch (System.Exception e)
		{
			print(e);
		}
	}

	IEnumerator PitchNextProjectile()
	{
		if (pitching)
		{
			yield break;
		}
		pitching = true;
		float timeout = Mathf.Lerp(startTimeoutRange.y, startTimeoutRange.x, totalBalls / 50f);

		yield return new WaitForSeconds(timeout);

		StartCoroutine(PlayWenMessage());

		yield return new WaitForSeconds(Mathf.Lerp(wenSoonTimeRange.y, wenSoonTimeRange.x, totalBalls / 50f));

		StartCoroutine(PlaySoonMessage());

		yield return new WaitForSeconds(timeout + XRandom.NextFloat(shootRandomTimeoutRange));

		ballMachineAnim.Play(ballMachineSpritesFire, false);
		ballMachinePlayerKiller.SetActive(true);

		yield return new WaitForSeconds(0.2f);

		SpawnProjectile();

		yield return new WaitForSeconds(0.3f);

		ballMachinePlayerKiller.SetActive(false);
		ballMachineAnim.Play(ballMachineSpritesIdle, false);
		pitching = false;
	}


	IEnumerator PlayWenMessage()
	{
		StopCoroutine(nameof(PlayWenMessage));
		audioManager.PlaySound(AudioManager.SoundID.messagePopup);
		wenMessageText.text = wenManager.GetRandomWenMessage();
		wenMessageText.transform.localScale = Vector3.zero;
		wenLineRendererObject.SetActive(true);
		Tween<float> scaleTween = new Tween<float>(0f, 0.1f, textTweenDuration, textTweenType);
		while (!scaleTween.IsEnded())
		{
			yield return new WaitForEndOfFrame();
			wenMessageText.transform.localScale = new Vector3(0.1f, scaleTween.Update(Time.deltaTime), 0.1f);
		}
		yield return new WaitForSeconds(Mathf.Lerp(textDisplayTimeRange.y, textDisplayTimeRange.x, totalBalls / 50f));
		wenMessageText.transform.localScale = Vector3.zero;
		wenLineRendererObject.SetActive(false);
	}


	IEnumerator PlaySoonMessage()
	{
		StopCoroutine(nameof(PlaySoonMessage));
		audioManager.PlaySound(AudioManager.SoundID.messagePopup);
		soonMessageText.transform.localScale = Vector3.zero;

		Tween<float> scaleTween = new Tween<float>(0f, 0.1f, textTweenDuration, textTweenType);
		while (!scaleTween.IsEnded())
		{
			yield return new WaitForEndOfFrame();
			soonMessageText.transform.localScale = new Vector3(0.1f, scaleTween.Update(Time.deltaTime), 0.1f);
		}
		yield return new WaitForSeconds(Mathf.Lerp(textDisplayTimeRange.y, textDisplayTimeRange.x, totalBalls / 50f));
		soonMessageText.transform.localScale = Vector3.zero;
	}


	IEnumerator AnimateScoreText()
	{
		float a = 0.09f;
		float b = 0.1f;

		Tween<float> scaleTween = new Tween<float>(a, b, 0.5f, TweenEaseType.CubicIn);

		while (!scaleTween.IsEnded())
		{
			yield return new WaitForEndOfFrame();
			scoreText.transform.localScale = new Vector3(scaleTween.Update(Time.deltaTime), scaleTween.Update(Time.deltaTime), scaleTween.Update(Time.deltaTime));
		}

		audioManager.PlaySound(AudioManager.SoundID.gainPoint);

		a = 0;
		b = 0.1f;

		Tween<float> scalePointMessageTween = new Tween<float>(a, b, 0.2f, TweenEaseType.CubicIn);

		while (!scalePointMessageTween.IsEnded())
		{
			yield return new WaitForEndOfFrame();
			scoreGainedText.transform.localScale = new Vector3(0.1f, scalePointMessageTween.Update(Time.deltaTime), 0.1f);
		}

		float timeBetweenRandomColors = 0.1f;
		int repeatTimes = 10;

		while (repeatTimes > 0)
		{
			yield return new WaitForSeconds(timeBetweenRandomColors);
			scoreGainedText.color = randomScoreGainedColors[Random.Range(0, randomScoreGainedColors.Count)];
			repeatTimes--;
		}

		scoreGainedText.transform.localScale = new Vector3(0, 0, 0);
	}

	void SpawnProjectile()
	{
		GameObject chosenProjectile = ballPrefab;
		float currentBombSpawnProbablity = Mathf.Lerp(bombSpawnProbabilityRange.x, bombSpawnProbabilityRange.y, (score - minScoreForBomb) / 20);
		if (score >= minScoreForBomb && XRandom.NextFloat() <= currentBombSpawnProbablity)
		{
			chosenProjectile = bombPrefab;
			hasLaunchedABomb = true;
			bombsFiredInARow++;

			if (bombsFiredInARow >= 5)
			{
				chosenProjectile = ballPrefab;
			}
		}
		else
		{
			bombsFiredInARow = 0;
		}

		var newProjectile = Instantiate(chosenProjectile, ballStartLocation);
		newProjectile.transform.localPosition = new Vector3(0, 0, 0);
		currentProjectile = newProjectile.GetComponent<Projectile>();
		currentProjectile.SetNewSpeed(Mathf.Min(maxBallSpeed, currentSpeedIncrease));
		if (!currentProjectile.isABomb)
		{
			totalBalls++;
		}

		audioManager.PlaySound(AudioManager.SoundID.projectileShoot);
	}
}

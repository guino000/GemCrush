using UnityEngine;
using System.Collections;
using GooglePlayGames.BasicApi;
using GooglePlayGames;
using UnityEngine.SocialPlatforms;

public class GPS : MonoBehaviour
{


	#region PUBLIC_VAR

	public string leaderboard
	{
		get;
		set;
	}

	#endregion

	#region DEFAULT_UNITY_CALLBACKS

	void Start()
	{
		PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
			.EnableSavedGames()
			.RequestEmail()
			.Build();

		PlayGamesPlatform.InitializeInstance(config);
		PlayGamesPlatform.DebugLogEnabled = true;
		PlayGamesPlatform.Activate();
	}

	#endregion

	#region BUTTON_CALLBACKS

	/// <summary>

	/// Login In Into Your Google+ Account

	/// </summary>

	public void LogIn()
	{
		Social.localUser.Authenticate((bool success) =>
		{
			if (success)
			{
				Debug.Log("Login Sucess");
			}
			else
			{
				Debug.Log("Login failed");
			}
		});
	}

    public bool CheckIfLoggedOn()
    {
        return ((PlayGamesPlatform)Social.Active).IsAuthenticated();
    }

	/// <summary>

	/// Shows All Available Leaderborad

	/// </summary>

	public void OnShowLeaderBoard()
	{
		PlayGamesPlatform.Instance.ShowLeaderboardUI(leaderboard); // Show current (Active) leaderboard
	}

	/// <summary>

	/// Adds Score To leader board

	/// </summary>

	public void OnAddScoreToLeaderBorad()
	{

		if (Social.localUser.authenticated)
		{

			Social.ReportScore(100, leaderboard, (bool success) =>

			{

				if (success)
				{

					Debug.Log("Update Score Success");



				}
				else
				{

					Debug.Log("Update Score Fail");

				}

			});

		}

	}

	/// <summary>

	/// On Logout of your Google+ Account

	/// </summary>

	public void OnLogOut()
	{

		((PlayGamesPlatform)Social.Active).SignOut();

	}

	#endregion
}

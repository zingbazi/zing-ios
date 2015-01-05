// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;

public class IFRemoteURL : MonoBehaviour {
	public string baseNonAPIUrl = "https://ifesserver-staging.herokuapp.com/";
	public string baseURL = "https://ifesserver-staging.herokuapp.com/api/v1/";
	
	public string gamesPath = "games";
	public string Games { get { return baseURL + gamesPath; } }

	public string questionsPath = "questions.json";
	public string Questions { get { return baseURL + questionsPath; } }

	public string categoriesPath = "categories.json";
	public string Categories { get { return baseURL + categoriesPath; } }

	public string highScoresPath = "highscores";
	public string HighScores { get { return baseURL + highScoresPath; } }

	public string signInPath = "sessions";
	public string SignIn { get { return baseURL + signInPath; } }

	public string signUpPath = "users";
	public string SignUp { get { return baseURL + signUpPath; } }
	
	public string usersPath = "users";
	public string Users { get { return baseURL + usersPath; } }
	
	public string eventLogPath = "events";
	public string EventLog { get { return baseURL + eventLogPath; } }
	
	public string challengesPath = "challenges";
	public string Challenges { get { return baseURL + challengesPath; } }
	
	public string newSurveyPath = "surveys/new";
	public string NewSurvey { get { return baseNonAPIUrl + newSurveyPath; } }

	public string surveyPath = "surveys";
	public string Survey { get { return baseURL + surveyPath; } }

	public string feedbackPath = "feedbacks";
	public string Feedback { get { return baseURL + feedbackPath; } }
	
	public string passwordResetPath = "password_resets";
	public string PasswordReset { get { return baseURL + passwordResetPath; } }
}

// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using SharpUnit;
using System.Data;
using Mono.Data.Sqlite;
using System.Text;

public class ModelTests : TestCase
{
	IFQuestionCategory politicsCategory;
//	IFQuestionCategory theLawCategory;
//	IFQuestion questionOne;
	
	public static bool ExecuteWWWCoroutine(IEnumerator coroutine, out WWW www)
	{
		www = null;
		while(coroutine.MoveNext()) {
			if(!(coroutine.Current is WWW)) {
				continue;
			}
			www = (WWW)coroutine.Current;
			float timeout = Time.realtimeSinceStartup + 5;
			
			while(!www.isDone) {
				if(Time.realtimeSinceStartup > timeout){
					Assert.False(true, "Network based test timed out");
					return false;
				}
			}
		}
		return true;
	}
	
	public override void SetUp()
	{	
//		politicsCategory = new IFQuestionCategory("Politics");
//		theLawCategory = new IFQuestionCategory("The Law");
		
//		string[] incorrect = new string[3] {"wrong", "wrong again", "and again"};
//		questionOne = new IFQuestion(0, 1, .5f, "This is the question", 
//									incorrect, "this is the correct answer", 
//									"it should be obvious", politicsCategory);
//		politicsCategory.AddQuestion(questionOne);
		
	}
	
//	[UnitTest]
//	public void TestCategoryMembership()
//	{
//		Assert.NotNull(politicsCategory.Questions);
//		Assert.Equal(1, politicsCategory.CountOfQuestions());
//		Assert.Equal(1, politicsCategory.Questions.Count);
//		Assert.True(politicsCategory.ContainsQuestion(questionOne));
//		
//		Assert.Equal(0, theLawCategory.CountOfQuestions());
//		
//		questionOne.Category = theLawCategory;
//		
//		Assert.Equal(1, theLawCategory.CountOfQuestions());
//		Assert.Equal(0, politicsCategory.CountOfQuestions());
//		Assert.True(theLawCategory.ContainsQuestion(questionOne));
//	}
	
//	[UnitTest]
//	public void TestLoadQuestionsFromJSON()
//	{
//		FileInfo fileInfo = new FileInfo("Assets/Tests/TestData/example_questions.json");
//		StreamReader reader = fileInfo.OpenText();
//		Assert.NotNull(reader, "The test data file example_qestions.json couldn't be openend.");
//		string json = reader.ReadToEnd();
//		List<IFQuestion> questions = IFQuestion.QuestionsFromJSON(json);
//		Assert.Equal(2, questions.Count);
//
//		IFQuestion q1 = questions[0];
//		Assert.Equal("Who was the first black president of the United States?", q1.Text);
//		Assert.Equal("Martin Luther King, Jr.", q1.IncorrectAnswers[0]);
//		Assert.Equal("Jimi Hendrix", q1.IncorrectAnswers[1]);
//		Assert.Equal("Tiger Woods", q1.IncorrectAnswers[2]);
//		Assert.Equal("Barack Obama", q1.CorrectAnswer);
//		Assert.Equal(1, q1.Level);
//		Assert.Equal(.5f, q1.Weight);
//		Assert.Equal("He was a congressman from Chicago", q1.Hint);
//		Assert.Equal("Politics", q1.Category.Name);
//		Assert.Equals(IFQuestionCategory.CategoryNamed("Politics"), q1.Category);
//	}
	
	[UnitTest]
	public void TestGetHighScores()
	{
		IFGame[] highScoringGames = IFGame.GetLocalTopGames(5);
		Assert.Equal(5, highScoringGames.Length);
		for(int i = 0; i < highScoringGames.Length - 1; i++) {
			Assert.True(highScoringGames[i].Score >= highScoringGames[i + 1].Score);
		}
	}
	
	[UnitTest]
	public void TestLevelRandomQuestionIdQueue()
	{
		int questionCount = 2;
		IFGameLevel level = new IFGameLevel(1, questionCount, 5f);
		
		int[] ids1 = level.GetRandomQuestionIdQueue().ToArray();
		int[] ids2 = level.GetRandomQuestionIdQueue().ToArray();
		
		Assert.Equal(ids1.Length, ids2.Length, "The random id queues were not the same length");
		Assert.False(ids1.Equals(ids2), "Got the same random ids");
	}
	
	[UnitTest]
	public void TestJSONHashForGameUpload()
	{
		int score = 180;
		IFGameLevel level = IFGameLevel.LevelWithRank(1);
		IFGame game = new IFGame(level, IFGame.GameMode.Normal);
		game.Score = score;
		
		List<IFAnswer> answers = new List<IFAnswer>();
		foreach(int id in level.GetRandomQuestionIdQueue()) {
			IFQuestion question = IFQuestion.QuestionWithIdentifier(id);			
			answers.Add(game.AnswerQuestion(question, id % 2 == 0, 5f));
		}
		
		Hashtable uploadHash = game.DataHashForUpload();
		
		Assert.Equal(score, Convert.ToInt32(uploadHash["score"]));
		ArrayList answerList = (ArrayList)uploadHash["answers"];
		Assert.NotNull(answerList);
		Assert.Equal(answers.Count, answerList.Count);
	}
	
	[UnitTest]
	public void TestEmailValidation()
	{
		string[] validEmails = { "test@empiricaldevelopment.com", "bob@gmail.com", "steve@apple.com", "larry+sergei@google.com", "my.email.address@domain.info" };
		string[] invalidEmails = { "an email with spaces@ domain with spaces.com", "hi@@email.com" };
		
		foreach(string email in validEmails) {
			Assert.True(IFUtils.IsValidEmail(email));
		}
		
		foreach(string email in invalidEmails) {
			Assert.False(IFUtils.IsValidEmail(email));
		}
		
	}
}

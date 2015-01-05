CREATE TABLE "questions" (
  "id" integer PRIMARY KEY,
  "text" TEXT,
  "incorrect1" TEXT,
  "incorrect2" TEXT,
  "incorrect3" TEXT,
  "correct" TEXT,
  "level" INTEGER,
  "weight" REAL,
  "hint" TEXT,
  "category" INTEGER REFERENCES "categories"("id")
);

CREATE TABLE "categories" (
  "id" INTEGER PRIMARY KEY,
  "name" TEXT
);

CREATE TABLE "category_question" (
  "id" INTEGER PRIMARY KEY,
  "category_id" INTEGER REFERENCES "categories"("id"),
  "question_id" INTEGER REFERENCES "questions"("id")
);

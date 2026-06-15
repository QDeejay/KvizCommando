import express from "express";

export default function questionsRouter(gameDb, adminDb) {
  const router = express.Router();

  router.get("/pending", async (req, res) => {
    try {
      const rows = await gameDb.all(
        "SELECT * FROM PendingQuestions WHERE Status = 'Pending'"
      );
      res.json(rows);
    } catch (err) {
      console.error("DB error:", err);
      res.status(500).json({ error: "Database read error" });
    }
  });

  return router;
}

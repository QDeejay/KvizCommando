import express from "express";
import cors from "cors";
import dotenv from "dotenv";
import { openDatabases } from "./config/db.js";
import questionsRouter from "./routes/questions.js";

dotenv.config();
const app = express();
app.use(cors());
app.use(express.json());

const { gameDb, userDb, adminDb } = await openDatabases();

app.use("/api/questions", questionsRouter(gameDb, adminDb));

const PORT = process.env.PORT || 4000;
app.listen(PORT, () => {
  console.log(`KC Admin Server running on http://localhost:${PORT}`);
});

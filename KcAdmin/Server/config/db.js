import sqlite3 from "sqlite3";
import { open } from "sqlite";
import path from "path";
import { fileURLToPath } from "url";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

export async function openDatabases() {
  const gameDbPath = path.resolve(__dirname, "../../../KvizCommando.Server/Game.db");
  const userDbPath = path.resolve(__dirname, "../../../KvizCommando.Server/GameUser.db");
  const adminDbPath = path.resolve(__dirname, "../data/admin.db");

  console.log("Opening databases:");
  console.log("Game DB:", gameDbPath);
  console.log("User DB:", userDbPath);
  console.log("Admin DB:", adminDbPath);

  const gameDb = await open({ filename: gameDbPath, driver: sqlite3.Database });
  const userDb = await open({ filename: userDbPath, driver: sqlite3.Database });
  const adminDb = await open({ filename: adminDbPath, driver: sqlite3.Database });

  return { gameDb, userDb, adminDb };
}

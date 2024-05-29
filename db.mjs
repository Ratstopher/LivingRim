import sqlite3 from 'sqlite3';
import { open } from 'sqlite';

export default async function initDb() {
    const db = await open({
        filename: './chat_log.db',
        driver: sqlite3.Database
    });

    await db.exec(`
        CREATE TABLE IF NOT EXISTS ChatLogs (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            characterId TEXT NOT NULL,
            pawnName TEXT NOT NULL,
            timestamp TEXT NOT NULL,
            interaction TEXT NOT NULL,
            response TEXT NOT NULL
        )
    `);

    return db;
}

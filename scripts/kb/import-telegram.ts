import fs from "node:fs";
import path from "node:path";
import {
  buildTelegramId,
  parseTelegramHtml,
  renderTelegramNotePage,
  slugTitleForMessage,
  WIKI_DIR_DUTCH_FILM_REVIEWS,
} from "./lib/import-telegram.js";
import { resolveSlugFilename } from "./lib/slug.js";

interface CliOptions {
  exportDir: string;
  repoRoot: string;
  dryRun: boolean;
  captureDate: string;
}

interface ManifestEntry {
  telegramId: string;
  messageId: string;
  title: string;
  wikiPath: string;
  status: "imported" | "skipped" | "error";
  reason?: string;
}

interface TelegramImportManifest {
  importDate: string;
  sourceExport: string;
  channelName: string;
  totals: {
    parsed: number;
    imported: number;
    skipped: number;
    error: number;
  };
  entries: ManifestEntry[];
  skipped: Array<{ messageId: string; reason: string }>;
}

function parseArgs(argv: string[]): CliOptions {
  const args = argv.slice(2);
  let exportDir = "";
  let dryRun = false;

  for (const arg of args) {
    if (arg === "--dry-run") {
      dryRun = true;
    } else if (!arg.startsWith("-") && !exportDir) {
      exportDir = arg;
    }
  }

  if (!exportDir) {
    console.error("Usage: npm run kb:import-telegram -- <export-dir> [--dry-run]");
    process.exit(1);
  }

  return {
    exportDir: path.resolve(exportDir),
    repoRoot: process.cwd(),
    dryRun,
    captureDate: new Date().toISOString().slice(0, 10),
  };
}

function ensureDir(dir: string, dryRun: boolean): void {
  if (!dryRun && !fs.existsSync(dir)) {
    fs.mkdirSync(dir, { recursive: true });
  }
}

function loadPriorManifest(manifestPath: string): {
  importedIds: Set<string>;
  entries: ManifestEntry[];
} {
  const importedIds = new Set<string>();
  const entries: ManifestEntry[] = [];

  if (!fs.existsSync(manifestPath)) {
    return { importedIds, entries };
  }

  try {
    const prior = JSON.parse(fs.readFileSync(manifestPath, "utf8")) as TelegramImportManifest;
    for (const entry of prior.entries) {
      entries.push(entry);
      if (entry.status === "imported") {
        importedIds.add(entry.telegramId);
      }
    }
  } catch {
    // ignore corrupt manifest
  }

  return { importedIds, entries };
}

function dedupeManifestEntries(entries: ManifestEntry[]): ManifestEntry[] {
  const byId = new Map<string, ManifestEntry>();
  const rank = (entry: ManifestEntry): number => {
    if (entry.status === "imported") return 3;
    if (entry.status === "error") return 2;
    return 1;
  };

  for (const entry of entries) {
    const existing = byId.get(entry.telegramId);
    if (!existing || rank(entry) > rank(existing)) {
      byId.set(entry.telegramId, entry);
    }
  }

  return [...byId.values()];
}

function appendNavigation(
  repoRoot: string,
  date: string,
  totals: TelegramImportManifest["totals"],
  channelName: string,
): void {
  if (totals.imported === 0) {
    return;
  }

  const indexPath = path.join(repoRoot, "wiki", "index.md");
  const logPath = path.join(repoRoot, "wiki", "log.md");

  const indexLine = `- ${date} — Telegram import (${channelName}): ${totals.imported} notes imported → see \`${WIKI_DIR_DUTCH_FILM_REVIEWS}/\``;
  const logLine = `- ${date} — telegram-import: ${totals.imported} imported, ${totals.skipped} skipped`;

  if (fs.existsSync(indexPath)) {
    const indexContent = fs.readFileSync(indexPath, "utf8");
    if (!indexContent.includes("Telegram import:")) {
      fs.appendFileSync(indexPath, `\n${indexLine}\n`, "utf8");
    }
  }

  if (fs.existsSync(logPath)) {
    const logContent = fs.readFileSync(logPath, "utf8");
    if (!logContent.includes("telegram-import:")) {
      fs.appendFileSync(logPath, `\n${logLine}\n`, "utf8");
    }
  }
}

async function main(): Promise<void> {
  const options = parseArgs(process.argv);
  const messagesPath = path.join(options.exportDir, "messages.html");

  if (!fs.existsSync(messagesPath)) {
    console.error(`messages.html not found in ${options.exportDir}`);
    process.exit(1);
  }

  const html = fs.readFileSync(messagesPath, "utf8");
  const parsed = parseTelegramHtml(html);

  const manifestDir = path.join(options.repoRoot, "artifacts", "kb-import");
  const manifestPath = path.join(manifestDir, `telegram-${options.captureDate}.manifest.json`);
  const wikiDir = path.join(options.repoRoot, WIKI_DIR_DUTCH_FILM_REVIEWS);

  ensureDir(manifestDir, options.dryRun);
  ensureDir(wikiDir, options.dryRun);

  const priorImported = loadPriorManifest(manifestPath);
  const manifestEntries: ManifestEntry[] = [...priorImported.entries];
  let imported = 0;
  let skipped = 0;
  let error = 0;

  console.log(
    `Parsed channel "${parsed.channelName}": ${parsed.messages.length} importable, ${parsed.skipped.length} skipped at parse`,
  );

  for (const message of parsed.messages) {
    const telegramId = buildTelegramId(message.messageId);

    if (priorImported.importedIds.has(telegramId)) {
      skipped += 1;
      manifestEntries.push({
        telegramId,
        messageId: message.messageId,
        title: message.title,
        wikiPath: "",
        status: "skipped",
        reason: "already_imported",
      });
      continue;
    }

    const slugInput = slugTitleForMessage(message.title, message.messageId);
    const filename = resolveSlugFilename(slugInput, wikiDir);
    const wikiPath = path.posix.join(WIKI_DIR_DUTCH_FILM_REVIEWS, filename);
    const fullPath = path.join(options.repoRoot, wikiPath.replace(/\//g, path.sep));

    if (options.dryRun) {
      imported += 1;
      manifestEntries.push({
        telegramId,
        messageId: message.messageId,
        title: message.title,
        wikiPath,
        status: "imported",
      });
      continue;
    }

    try {
      const content = renderTelegramNotePage(message);
      fs.writeFileSync(fullPath, content, "utf8");
      imported += 1;
      manifestEntries.push({
        telegramId,
        messageId: message.messageId,
        title: message.title,
        wikiPath,
        status: "imported",
      });
    } catch (cause) {
      error += 1;
      manifestEntries.push({
        telegramId,
        messageId: message.messageId,
        title: message.title,
        wikiPath: "",
        status: "error",
        reason: cause instanceof Error ? cause.message : "write_failed",
      });
    }
  }

  const totals = {
    parsed: parsed.messages.length,
    imported,
    skipped: skipped + parsed.skipped.length,
    error,
  };

  const manifest: TelegramImportManifest = {
    importDate: options.captureDate,
    sourceExport: options.exportDir,
    channelName: parsed.channelName,
    totals,
    entries: dedupeManifestEntries(manifestEntries),
    skipped: parsed.skipped,
  };

  if (!options.dryRun) {
    fs.writeFileSync(manifestPath, `${JSON.stringify(manifest, null, 2)}\n`, "utf8");
    appendNavigation(options.repoRoot, options.captureDate, totals, parsed.channelName);
  } else {
    console.log("(dry-run: manifest not written, no wiki files created)");
  }

  console.log(`  imported: ${imported}`);
  console.log(`  skipped: ${totals.skipped}`);
  console.log(`  error: ${error}`);
  if (!options.dryRun) {
    console.log(`  manifest: ${manifestPath}`);
  }
}

main().catch((err) => {
  console.error(err);
  process.exit(1);
});

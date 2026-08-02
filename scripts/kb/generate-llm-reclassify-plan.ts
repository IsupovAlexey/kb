/**
 * One-shot generator for LLM-authored reclassify plan. Run once to refresh JSON artifact.
 * Classification decisions are human/LLM curated — not heuristic rules at apply time.
 */
import fs from "node:fs";
import path from "node:path";

type Dest = string;

function fm(from: string, dest: Dest, reason: string) {
  const base = from.split("/").pop()!;
  const to = dest.endsWith("/") ? dest + base : dest;
  return { from, to, reason };
}

const fileMoves: Array<{ from: string; to: string; reason: string }> = [];

function add(from: string, dest: Dest, reason: string) {
  fileMoves.push(fm(from, dest, reason));
}

// --- utilities (53) → thematic folders ---
const u = "wiki/bookmarks/utilities/";
add(u + "1-vet-line-veterinary-clinic-facebook.md", "wiki/bookmarks/pets/", "Armenian vet clinic Facebook post");
add(u + "5-best-lg-monitors-2023-reviews-rtings-com.md", "wiki/bookmarks/hardware/", "Monitor reviews");
add(u + "670-i-way-transferseveryday.md", "wiki/bookmarks/personal/travel/", "Airport transfer service");
add(u + "airquality-am.md", "wiki/bookmarks/personal/armenia/", "Yerevan air quality");
add(u + "am4-vcore-vrm-ratings-v1-2-2019-07-10-google.md", "wiki/bookmarks/hardware/", "AM4 VRM spreadsheet");
add(u + "b550-vrm-db-sheet-ver-1-4-google.md", "wiki/bookmarks/hardware/", "B550 VRM spreadsheet");
add(u + "browse-cheapshark.md", "wiki/bookmarks/games/", "Game price tracker");
add(u + "build-better-cache-teamcity.md", "wiki/bookmarks/programming/devops/", "TeamCity caching");
add(u + "calendar-2021-2022-snooker-org.md", "wiki/bookmarks/interests/sports/", "Snooker calendar");
add(u + "catalyst-moodle-auth-userkey-log-moodle-using-one-time-user-key.md", "wiki/bookmarks/programming/devops/", "Moodle auth plugin");
add(u + "cobalt.md", "wiki/bookmarks/programming/", "Cobalt media framework");
add(u + "companies-ranked-market-cap-companiesmarketcap-com.md", "wiki/bookmarks/finance/", "Market cap rankings");
add(u + "dylanraga-win11hdr-srgb-to-gamma2-2-icm-transform-windows-11-s-v.md", "wiki/bookmarks/hardware/", "Windows HDR ICC profile");
add(u + "e-trade-complete-view.md", "wiki/bookmarks/finance/", "E*TRADE portfolio");
add(u + "encyclopedia-pathologica.md", "wiki/bookmarks/interests/", "Niche encyclopedia");
add(u + "explainer-armenia-s-new-income-declaration-system-step-by-step-g.md", "wiki/bookmarks/personal/armenia/", "Armenia tax declaration guide");
add(u + "floorplanner-i155mk-2.md", "wiki/bookmarks/personal/home/", "Room layout tool");
add(u + "global-keyboard-shortcut-support-firefox.md", "wiki/bookmarks/tools/", "Firefox shortcut support");
add(u + "guide-sexing-budgies-r-budgies.md", "wiki/bookmarks/pets/", "Budgie sexing guide");
add(u + "how-disable-steamvr-from-automatically-starting-up-when-launchin.md", "wiki/bookmarks/games/", "SteamVR autostart");
add(u + "how-get-your-bird-eat-pellets-seeds-vs-pellets-pellet-diet-parro.md", "wiki/bookmarks/pets/", "Parrot pellet diet");
add(u + "insta-sell-all-rsus-always-philip-su-molochinations.md", "wiki/bookmarks/careers/equity/", "RSU selling strategy");
add(u + "jlevy-og-equity-compensation-stock-options-rsus-taxes-read-lates.md", "wiki/bookmarks/careers/equity/", "Equity compensation guide");
add(u + "last-fm-top-songs-time-period-spotlistr.md", "wiki/bookmarks/music/", "Last.fm playlist tool");
add(u + "less-chat-more-answer-site-ai-chatbots-need-get-point-nn-g.md", "wiki/bookmarks/programming/ai/", "AI chatbot UX article");
add(u + "lg-43ud79-b-review-rtings-com.md", "wiki/bookmarks/hardware/", "Monitor review");
add(u + "meduza.md", "wiki/bookmarks/media/news/", "Meduza article");
add(u + "minisforum.md", "wiki/bookmarks/hardware/", "Mini PC vendor");
add(u + "moodle-auth-userkey-readme-md-moodle-33plus-catalyst-moodle-auth.md", "wiki/bookmarks/programming/devops/", "Moodle auth readme");
add(u + "national-library-armenia-mcp-server-r-armenia.md", "wiki/bookmarks/languages/armenian/", "Armenia library MCP");
add(u + "nometa.md", "wiki/bookmarks/social/", "Anti meta tags movement");
add(u + "oh-joy-sex-toy-anal-sex-preparation.md", "wiki/bookmarks/adult/", "Sex education comic");
add(u + "online-mime-headers-decoder-rfc-2047.md", "wiki/bookmarks/tools/", "MIME header decoder");
add(u + "online-programma-gemist-downloader.md", "wiki/bookmarks/languages/dutch/", "Dutch TV replay downloader");
add(u + "open-badge-designer.md", "wiki/bookmarks/tools/", "Open Badge tool");
add(u + "photopea-online-photo-editor.md", "wiki/bookmarks/tools/", "Online photo editor");
add(u + "piratebay-proxy-working-pirate-bay-proxy-sites-mirrors.md", "wiki/bookmarks/games/", "Torrent proxy list");
add(u + "punycode-punycode.md", "wiki/bookmarks/tools/", "Punycode converter");
add(u + "ssds-google.md", "wiki/bookmarks/hardware/", "SSD comparison sheet");
add(u + "steam-library-filters.md", "wiki/bookmarks/games/", "Steam library filters");
add(u + "tailscale-secure-connectivity-ai-iot-multi-cloud.md", "wiki/bookmarks/programming/devops/", "Tailscale VPN");
add(u + "topdeck.md", "wiki/bookmarks/games/", "Game daily-limit reset tool (not MTG)");
add(u + "torrent-search-engine-bt4g.md", "wiki/bookmarks/games/", "Torrent search");
add(u + "untitled-2.md", "wiki/bookmarks/personal/health/", "Dental prosthesis forum thread");
add(u + "untitled-3.md", "wiki/bookmarks/languages/armenian/", "Paruyr Sevak poetry");
add(u + "untitled-4.md", "wiki/bookmarks/pets/", "Forced parrot feeding forum");
add(u + "untitled-5.md", "wiki/bookmarks/personal/armenia/", "Armenia utility outages");
add(u + "untitled-6.md", "wiki/bookmarks/personal/armenia/", "Armenia citizenship test");
add(u + "untitled.md", "wiki/bookmarks/interests/", "Russian meme/gif collection");
add(u + "using-codegeex-as-github-copilot-alternative-logrocket-blog.md", "wiki/bookmarks/programming/ai/", "CodeGeeX Copilot alternative");
add(u + "vetexpert.md", "wiki/bookmarks/pets/", "Exotic vet clinic Armenia");
add(u + "workpermit-web.md", "wiki/bookmarks/careers/immigration/", "Work permit info");
add(u + "zaka-zaka-com-steam-steam-origin.md", "wiki/bookmarks/games/", "Game key marketplace");

// --- toolbar/misc (25) ---
const m = "wiki/bookmarks/toolbar/misc/";
add(m + "books-baum-l-frank-lyman-frank-sorted-popularity-project-gutenbe.md", "wiki/bookmarks/books/", "Gutenberg Baum catalog");
add(m + "connectmyhandy.md", "wiki/bookmarks/tools/", "Phone connect tool");
add(m + "dune-spice-opera-2024-remaster-lp-exxos-stephane-picq-philippe-u.md", "wiki/bookmarks/music/", "Dune Spice Opera album");
add(m + "following-bluesky.md", "wiki/bookmarks/social/", "Bluesky following feed");
add(m + "free-vr-scripts-list-google.md", "wiki/bookmarks/games/", "VR scripts spreadsheet");
add(m + "ivan-bodhidharma-igor-i-flickr-2.md", "wiki/bookmarks/social/", "Flickr photographer profile");
add(m + "ivan-bodhidharma-igor-i-flickr.md", "wiki/bookmarks/social/", "Flickr photographer profile");
add(m + "kagi-translate.md", "wiki/bookmarks/tools/", "Kagi Translate");
add(m + "morow-m3u.md", "wiki/bookmarks/tools/", "IPTV playlist");
add(m + "nadya-whatever-x-34-x.md", "wiki/bookmarks/social/", "X/Twitter post");
add(m + "nadya-whatever-x-ddpanier-4-x.md", "wiki/bookmarks/social/", "X/Twitter post");
add(m + "novel-universal-bypass-all-major-llms.md", "wiki/bookmarks/programming/ai/", "LLM bypass tool");
add(m + "padovan-grandmix-400.md", "wiki/bookmarks/pets/", "Bird food product");
add(m + "plati-market-digital-goods-marketplace.md", "wiki/bookmarks/games/", "Digital goods marketplace");
add(m + "qwen.md", "wiki/bookmarks/programming/ai/", "Qwen LLM");
add(m + "sankeymatic-make-beautiful-flow-diagrams.md", "wiki/bookmarks/tools/", "Sankey diagram tool");
add(m + "schema-json-resume.md", "wiki/bookmarks/careers/job-search/", "JSON Resume schema");
add(m + "sign-register.md", "wiki/bookmarks/languages/armenian/", "E-mova Ukrainian language portal");
add(m + "sound-morning-sleep-son-2-right-now-24-f-flickr.md", "wiki/bookmarks/adult/", "Adult Flickr photo");
add(m + "tanstack-high-quality-open-source-software-web-developers.md", "wiki/bookmarks/programming/", "TanStack libraries");
add(m + "unstringify-json-online-json-tools.md", "wiki/bookmarks/tools/", "JSON unstringify tool");
add(m + "viruse-project-black-out-1.md", "wiki/bookmarks/adult/", "Adult video series");
add(m + "we-cover-letters.md", "wiki/bookmarks/careers/job-search/", "Cover letter resource");
add(m + "workpermit-web.md", "wiki/bookmarks/careers/immigration/", "Work permit (duplicate bookmark)");
add(m + "zaka-zaka-com.md", "wiki/bookmarks/games/", "Game key shop");

// --- toolbar root ---
const t = "wiki/bookmarks/toolbar/";
add(t + "e-trade-stock-plan-overview.md", "wiki/bookmarks/careers/equity/", "E*TRADE stock plan");
add(t + "google-keep.md", "wiki/bookmarks/personal/", "Google Keep notes");
add(t + "https-mcs-citizenship-am.md", "wiki/bookmarks/personal/armenia/", "Armenia citizenship service");
add(t + "ino.md", "wiki/bookmarks/personal/", "Ino bookmark");
add(t + "linkedin.md", "wiki/bookmarks/social/", "LinkedIn");
add(t + "reddit.md", "wiki/bookmarks/social/", "Reddit");
add(t + "twitch.md", "wiki/bookmarks/social/", "Twitch");
add(t + "youtube.md", "wiki/bookmarks/social/", "YouTube");
add(t + "untitled.md", "wiki/bookmarks/interests/", "FNK bookmark");

// --- careers misplaced ---
add("wiki/bookmarks/careers/harry-the-nerd-interview-notes-questions-real-interview-question.md", "wiki/bookmarks/careers/interviews/", "Interview notes compilation");
add("wiki/bookmarks/careers/how-approach-interviewing-juniors-experienceddevs.md", "wiki/bookmarks/careers/interviews/", "Junior interview approach");
const js = "wiki/bookmarks/careers/job-search/";
add(js + "factory-patterns-abstract-factory-pattern-codeproject.md", "wiki/bookmarks/programming/", "Design patterns article — not job search");
add(js + "programming-stuff-di-property-injection.md", "wiki/bookmarks/programming/", "DI property injection — dev topic");
add(js + "living-netherlands-ind.md", "wiki/bookmarks/careers/immigration/", "Living in Netherlands IND");
add(js + "public-register-recognised-sponsors-ind.md", "wiki/bookmarks/careers/immigration/", "IND recognised sponsors");
add(js + "public-register-regular-labour-highly-skilled-migrants-ind.md", "wiki/bookmarks/careers/immigration/", "IND labour migrant register");

// --- magic root → magic/mtg ---
for (const name of [
  "ari-s-core-set-2019-limited-review.md",
  "coolstuffinc-com-online-retailer-board-games-mtg-many-other-coll.md",
  "deathsie-s-limited-tierlists-master-collection.md",
  "draftaholics-anonymous.md",
  "frank-analysis-how-many-colored-mana-sources-do-you-need-consist.md",
  "gfcards-ru-topdeck-ru.md",
  "how-many-colored-mana-sources-do-you-need-consistently-cast-your.md",
  "kci-deck-guide.md",
  "less-practical-magic-rna-draft-meeting-part-1-archetypes.md",
  "magic-deck-building-statistics-eternal-central.md",
  "starcitygames-com-what-do-when-you-don-t-know-what-do.md",
  "starcitygames-com-you-can-play-modern-s-best-decks-here-s-how.md",
  "top-8-predictor-swiss-rounds-tournament-tool-limitedinformation.md",
  "welcome-r-magicalchemy-deck-lists-ideas-r-magicalchemy.md",
]) {
  add(`wiki/bookmarks/magic/${name}`, "wiki/bookmarks/magic/mtg/", "Consolidate MTG bookmarks under magic/mtg");
}

// --- adult/mapnaam → adult (flatten Dutch folder name) ---
const mapnaam = "wiki/bookmarks/adult/mapnaam/";
for (const name of fs.readdirSync(path.join(process.cwd(), mapnaam))) {
  if (!name.endsWith(".md")) continue;
  add(mapnaam + name, "wiki/bookmarks/adult/", "Flatten mapnaam (Dutch folder label) into adult");
}

// --- stray root bookmark ---
add("wiki/bookmarks/get-bookmark-add-ons.md", "wiki/bookmarks/tools/", "Firefox bookmark add-ons");

const directoryMoves = [
  {
    from: "wiki/bookmarks/toolbar/jobs",
    to: "wiki/bookmarks/careers/job-search",
    reason: "Merge toolbar quick-access job links into careers/job-search",
  },
  {
    from: "wiki/bookmarks/toolbar/nadpo",
    to: "wiki/bookmarks/work/nadpo",
    reason: "Work infrastructure out of Firefox toolbar folder",
  },
  {
    from: "wiki/bookmarks/work/nadpo/dokumentaciya-po-sdo-i-b24",
    to: "wiki/bookmarks/work/nadpo/bitrix-yookassa-docs",
    reason: "English alias for Bitrix24/YooKassa documentation folder",
  },
  {
    from: "wiki/bookmarks/work/nadpo/centr-karery",
    to: "wiki/bookmarks/work/nadpo/careers-center",
    reason: "English alias for careers center folder",
  },
];

const plan = {
  note: "LLM-authored full reclassify pass. Apply via: npx tsx scripts/kb/apply-llm-reclassify.ts",
  directoryMoves,
  fileMoves,
};

const out = path.join(
  process.cwd(),
  "artifacts/tacos-work/firefox-bookmark-import/llm-reclassify-plan.json",
);
fs.writeFileSync(out, JSON.stringify(plan, null, 2));
console.log(`Wrote ${fileMoves.length} file moves and ${directoryMoves.length} directory moves to ${out}`);

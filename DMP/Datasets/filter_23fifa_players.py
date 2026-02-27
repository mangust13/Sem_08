import os
import pandas as pd

INPUT_CSV = "all_players.csv"
OUTPUT_CSV = "players_fifa23.csv"
TARGET_VERSION = 23
CHUNK_SIZE = 200_000

DROP_COLUMNS = [
    'player_id', 'player_url', 'fifa_version', 'fifa_update', 'fifa_update_date',
    'long_name', 'dob', 'league_id', 'league_name', 'league_level', 'club_team_id',
    'club_name', 'club_position', 'club_jersey_number', 'club_loaned_from',
    'club_joined_date', 'club_contract_valid_until_year', 'nationality_id',
    'nationality_name', 'nation_team_id', 'nation_position', 'nation_jersey_number',
    'real_face', 'release_clause_eur', 'ls', 'st', 'rs', 'lw', 'lf', 'cf', 'rf', 'rw',
    'lam', 'cam', 'ram', 'lm', 'lcm', 'cm', 'rcm', 'rm', 'lwb', 'ldm', 'cdm', 'rdm',
    'rwb', 'lb', 'lcb', 'cb', 'rcb', 'rb', 'gk', 'player_face_url'
]

if os.path.exists(OUTPUT_CSV):
    print(f"File already exists: {OUTPUT_CSV}. Skipping...")
else:
    first = True

    for chunk in pd.read_csv(INPUT_CSV, chunksize=CHUNK_SIZE):
        filtered = chunk[chunk["fifa_version"] == TARGET_VERSION]

        if not filtered.empty:
            filtered = filtered.drop(columns=DROP_COLUMNS, errors="ignore")

            filtered.to_csv(
                OUTPUT_CSV,
                mode="w" if first else "a",
                index=False,
                header=first
            )
            first = False

    print(f"Saved to: {OUTPUT_CSV}")

df = pd.read_csv(OUTPUT_CSV, nrows=1)
print("All columns in output dataset:")
print(df.columns.tolist())


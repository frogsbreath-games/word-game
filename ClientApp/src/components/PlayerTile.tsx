import * as React from "react";
import { ReactComponent as PlayerIcon } from "../assets/PlayerIcon.svg";
import { ReactComponent as SwapIcon } from "../assets/SwapIcon.svg";
import { ReactComponent as TrashIcon } from "../assets/TrashIcon.svg";
import { ReactComponent as QuitIcon } from "../assets/CancelIcon.svg";
import CharacterImg from "./CharacterImg";
import * as GameStore from "../store/Game";
import styles from "./PlayerTile.module.css";

type PlayerTileProps = {
  player: GameStore.Player;
  localPlayer: GameStore.Player;
  gameActions: GameStore.GameActions;
  availableCharacters: GameStore.Character[];
  key: number;
  code: string;
  swapTeams: (player: GameStore.Player) => void;
  leaveGame: () => void;
  deleteBot: (playerNumber: number) => void;
  changeCharacter: (player: GameStore.Player, number: number) => void;
};

const PlayerTile = ({
  player,
  gameActions,
  localPlayer,
  availableCharacters,
  swapTeams,
  leaveGame,
  deleteBot,
  changeCharacter,
  code,
}: PlayerTileProps) => (
  <div>
    <div className={styles[player.team + "Tile"]}>
      <div className={styles.name}>
        <h5 style={{ color: "white", marginBottom: 0 }}>{player.name}</h5>
        {/* <h3 style={{ color: "white" }}>
          {player.type === null ? "Pick a player!" : player.type}
        </h3> */}
      </div>
      <div className={styles.character}>
        {player.number === localPlayer.number && (
          <PlayerIcon className={styles.icon} />
        )}
        <CharacterImg number={player.characterNumber} />
      </div>
      <div className={styles.select}>
        <select
          value={player.characterNumber !== null ? player.characterNumber : -1}
          onChange={(event) => changeCharacter(player, +event.target.value)}
          disabled={
            player.number !== localPlayer.number &&
            localPlayer.role !== "organizer"
          }
          className={styles.upperCase}
        >
          <option value={-1}>--None--</option>
          {player.characterNumber !== null && (
            <option value={player.characterNumber}>
              {player.type +
                " " +
                player.characterName +
                " " +
                (player.type === "cultist" ? " 👿" : " 🤠")}
            </option>
          )}
          {availableCharacters
            .filter((c) => c.team === player.team)
            .map((c) => (
              <option value={c.number}>
                {c.type +
                  " " +
                  c.name +
                  " " +
                  (c.type === "cultist" ? " 👿" : " 🤠")}
              </option>
            ))}
        </select>
      </div>

      <div className={styles.buttons}>
        {(localPlayer.role === "organizer" ||
          player.number === localPlayer.number) && (
          <button
            className={styles.button}
            type="button"
            onClick={() => swapTeams(player)}
            style={{ marginTop: "10px" }}
          >
            <SwapIcon height={25} style={{ opacity: 0.5, fill: "white" }} />
          </button>
        )}
        {gameActions.canDeleteBot && player.role === "bot" && (
          <button
            className={styles.button}
            type="button"
            onClick={() => deleteBot(player.number)}
            style={{ marginTop: "10px", marginLeft: "5px" }}
          >
            <TrashIcon height={25} style={{ opacity: 0.5, fill: "white" }} />
          </button>
        )}
        {player.number === localPlayer.number &&
          localPlayer.role !== "organizer" && (
            <button
              className={styles.button}
              type="button"
              onClick={() => leaveGame()}
              style={{ marginLeft: "10px", marginTop: "10px" }}
            >
              <QuitIcon width={25} style={{ opacity: 0.5, fill: "white" }} />
            </button>
          )}
      </div>
    </div>
  </div>
);

export default PlayerTile;

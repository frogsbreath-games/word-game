import * as React from "react";
import { ReactComponent as PlayerIcon } from "../assets/PlayerIcon.svg";
import { ReactComponent as SwapIcon } from "../assets/SwapIcon.svg";
import { ReactComponent as TrashIcon } from "../assets/TrashIcon.svg";
import { ReactComponent as QuitIcon } from "../assets/CancelIcon.svg";
import researcher from "../assets/researcher.png";
import researcher2 from "../assets/researcher2.png";
import researcher3 from "../assets/researcher3.png";
import researcher4 from "../assets/researcher4.png";
import cultist from "../assets/Cultist.png";
import sorceress from "../assets/Cultist2.png";
import * as GameStore from "../store/Game";
import styles from "./PlayerTile.module.css";

type PlayerTileProps = {
  player: GameStore.Player;
  localPlayer: GameStore.Player;
  gameActions: GameStore.GameActions;
  key: number;
  code: string;
  swapTeams: (player: GameStore.Player) => void;
  leaveGame: () => void;
  deleteBot: (playerNumber: number) => void;
  changeRole: (player: GameStore.Player, type: GameStore.PlayerType) => void;
};

const PlayerTile = ({
  player,
  gameActions,
  localPlayer,
  swapTeams,
  leaveGame,
  deleteBot,
  changeRole,
  code
}: PlayerTileProps) => (
  <div>
    <div className={styles[player.team + "Tile"]}>
      <div className={styles.name}>
        <h5 style={{ color: "white" }}>{player.name}</h5>
      </div>

      <div className={styles.character}>
        {player.number === localPlayer.number && (
          <PlayerIcon className={styles.icon} />
        )}
        {player.type === "researcher" ? (
          <img
            src={player.number % 2 === 0 ? researcher4 : researcher3}
            style={{ maxHeight: "100%", maxWidth: "100%" }}
            alt="Researcher"
          />
        ) : (
          <img
            src={player.number % 2 === 0 ? cultist : sorceress}
            style={{ maxHeight: "100%", maxWidth: "100%" }}
            alt="Cultist"
          />
        )}
      </div>
      <div className={styles.select}>
        <select
          value={player.type}
          onChange={event =>
            changeRole(
              player,
              event.target.value === "researcher" ? "researcher" : "cultist"
            )
          }
        >
          <option value="researcher">Researcher</option>
          <option value="cultist">Cultist</option>
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
            <SwapIcon width={25} style={{ opacity: 0.5, fill: "white" }} />
          </button>
        )}
        {gameActions.canDeleteBot && player.role === "bot" && (
          <button
            className={styles.button}
            type="button"
            onClick={() => deleteBot(player.number)}
            style={{ marginTop: "10px", marginLeft: "5px" }}
          >
            <TrashIcon width={25} style={{ opacity: 0.5, fill: "white" }} />
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

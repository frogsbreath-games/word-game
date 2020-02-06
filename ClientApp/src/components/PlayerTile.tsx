import * as React from "react";
import { ReactComponent as PlayerIcon } from "../assets/PlayerIcon.svg";
import { ReactComponent as SwapIcon } from "../assets/SwapIcon.svg";
import { ReactComponent as TrashIcon } from "../assets/TrashIcon.svg";
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
      <div className={styles.tileLabel}>
        <h5>{player.name}</h5>
        {player.number === localPlayer.number && (
          <PlayerIcon
            //Need to know how to do this better
            className={player.team === "red" ? styles.redIcon : styles.blueIcon}
          />
        )}
          <select value={player.type} onChange={(event) => changeRole(player, event.target.value === "researcher" ? "researcher" : "cultist")}>
            <option value="researcher">Researcher</option>
            <option value="cultist">Cultist</option>
          </select>
      </div>
      {(localPlayer.role === "organizer" || player.number === localPlayer.number) && (
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
      {player.number === localPlayer.number && localPlayer.role !== "organizer" && (
        <button
          className={styles.button}
          type="button"
          onClick={() => leaveGame()}
          style={{ marginLeft: "10px", marginTop: "10px" }}
        >
          Leave Game
        </button>
      )}
    </div>
  </div>
);

export default PlayerTile;

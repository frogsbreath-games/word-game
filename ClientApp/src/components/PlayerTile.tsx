import * as React from "react";
import { ReactComponent as PlayerIcon } from "../assets/PlayerIcon.svg";
import { ReactComponent as SwapIcon } from "../assets/SwapIcon.svg";
import { ReactComponent as TrashIcon } from "../assets/TrashIcon.svg";
import * as GameStore from "../store/Game";
import { red, blue } from "../constants/ColorConstants";
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
  changeRole: (player: GameStore.Player) => void;
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
    <div
      className={
        player.team === "red" ? styles.containerRed : styles.containerBlue
      }
    >
      <div className={styles.tileLabel}>
        <h5>{player.name}</h5>
        {player.number === localPlayer.number && (
          <PlayerIcon
            //Need to know how to do this better
            className={player.team === "red" ? styles.redIcon : styles.blueIcon}
          />
        )}
        <div className="form-check">
          <input
            type="checkbox"
            className="form-check-input"
            checked={player.isSpyMaster}
            onClick={() => changeRole(player)}
          />
          <label className="form-check-label">Is Spy Master?</label>
        </div>
        {/* <p>{player.isSpyMaster ? "Spy Master" : "Agent"}</p> */}
      </div>
      {(localPlayer.isOrganizer || player.number === localPlayer.number) && (
        <button
          className="btn btn-secondary"
          type="button"
          onClick={() => swapTeams(player)}
          style={{ marginTop: "10px" }}
        >
          <SwapIcon width={25} style={{ opacity: 0.5, fill: "white" }} />
        </button>
      )}
      {gameActions.canDeleteBot && player.isBot && (
        <button
          className="btn btn-secondary"
          type="button"
          onClick={() => deleteBot(player.number)}
          style={{ marginTop: "10px", marginLeft: "5px" }}
        >
          <TrashIcon width={25} style={{ opacity: 0.5, fill: "white" }} />
        </button>
      )}
      {player.number === localPlayer.number && !localPlayer.isOrganizer && (
        <button
          className="btn btn-danger"
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

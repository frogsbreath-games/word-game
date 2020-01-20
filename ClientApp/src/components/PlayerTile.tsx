import * as React from "react";
import { ReactComponent as PlayerIcon } from "../assets/PlayerIcon.svg";
import { ReactComponent as SwapIcon } from "../assets/SwapIcon.svg";
import { ReactComponent as TrashIcon } from "../assets/TrashIcon.svg";
import * as GameStore from "../store/Game";
import { red, blue } from "../constants/ColorConstants";

type PlayerTileProps = {
  player: GameStore.Player;
  localPlayer: GameStore.Player;
  gameActions: GameStore.GameActions;
  key: number;
  code: string;
  swapTeams: (player: GameStore.Player) => void;
  leaveGame: () => void;
  deleteBot: (playerNumber: number) => void;
};

const PlayerTile = ({
  player,
  gameActions,
  localPlayer,
  swapTeams,
  leaveGame,
  deleteBot,
  code
}: PlayerTileProps) => (
  <div>
    <div
      style={{
        borderRadius: "3px",
        margin: "15px",
        padding: "10px",
        backgroundColor: player.team === "red" ? red : blue,
        boxShadow:
          "0 1px 3px rgba(0, 0, 0, 0.24), 0 1px 3px rgba(0, 0, 0, 0.36)"
      }}
    >
      <div>
        <div
          style={{
            backgroundColor: "rgba(255, 255, 255, 0.75)",
            padding: "10px",
            borderRadius: "5px",
            boxShadow:
              "0 1px 3px rgba(255, 255, 255, 0.24), 0 1px 3px rgba(255, 255, 255, 0.36)"
          }}
        >
          <h5>{player.name}</h5>
          {player.number === localPlayer.number && (
            <PlayerIcon
              //Need to know how to do this better
              className="justify-content-end"
              style={{
                opacity: 0.5,
                width: "50px",
                float: "right",
                marginTop: "-25px",
                fill: player.team === "red" ? red : blue
              }}
            />
          )}
          <p>{player.isSpyMaster ? "Spy Master" : "Agent"}</p>
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
  </div>
);

export default PlayerTile;

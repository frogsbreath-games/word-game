import * as React from "react";
import { connect } from "react-redux";
import * as GameStore from "../store/Game";
import { ApplicationState } from "../store";
import { Redirect } from "react-router";
import { red, blue, tan, black, grey } from "../constants/ColorConstants";
import "./Game.css";
import { ReactComponent as RevealedIcon } from "../assets/RevealedIcon.svg";
import { ReactComponent as PlayerIcon } from "../assets/PlayerIcon.svg";

// At runtime, Redux will merge together...
type GameProps = GameStore.GameState & // ... state we've requested from the Redux store
  typeof GameStore.actionCreators;

type State = { hintWord: string; wordCount: number };

function getColor(color: string, isRevealed: boolean) {
  if (!isRevealed) {
    return grey;
  }
  switch (color) {
    case "red":
      return red;
    case "blue":
      return blue;
    case "neutral":
      return tan;
    default:
      return black;
  }
}

type TeamTileProps = {
  team: string;
  tilesRemaining: number;
  isTeamsTurn: boolean;
};

const TeamTile = ({ team, tilesRemaining, isTeamsTurn }: TeamTileProps) => (
  <div className={"team-tile " + team}>
    <h3 className={"team-label " + team}>{team + " Team"}</h3>
    <div>
      <h3>Remaining Tiles: {tilesRemaining}</h3>
      {isTeamsTurn && <h6 className="light-label">Current Turn</h6>}
    </div>
  </div>
);

type PlayerTrackerProps = {
  playerName: string;
  team: string;
};

const PlayerTracker = ({ playerName, team }: PlayerTrackerProps) => (
  <div className={"player-tracker " + team}>
    <h6>{playerName}</h6>
    <div className={"player-icon " + team}>
      <PlayerIcon className="player-badge" />
    </div>
  </div>
);

type GameTileProps = {
  wordTile: GameStore.WordTile;
  localPlayer: GameStore.Player;
  turnStatus: string;
  handleVoteWord: (word: string, isRevealed: boolean) => void;
};

const GameTile = ({
  wordTile,
  localPlayer,
  turnStatus,
  handleVoteWord
}: GameTileProps) => (
  <div
    onClick={() => handleVoteWord(wordTile.word, wordTile.isRevealed)}
    className={
      "word-tile " +
      (wordTile.isRevealed || localPlayer.isSpyMaster ? wordTile.team : "grey")
    }
  >
    <h6 className="light-label">{wordTile.word}</h6>
    {turnStatus === "guessing" &&
      !localPlayer.isSpyMaster &&
      !wordTile.isRevealed && (
        <h6 className="light-label">Votes: {wordTile.votes.length}</h6>
      )}
    {localPlayer.isSpyMaster && wordTile.isRevealed && (
      <div className="light-label">
        <RevealedIcon className="reveal-icon" />
      </div>
    )}
  </div>
);

class Game extends React.PureComponent<GameProps, State> {
  constructor(props: GameProps) {
    super(props);
    this.state = { hintWord: "", wordCount: 0 };

    this.handleWordChange = this.handleWordChange.bind(this);
    this.handleCountChange = this.handleCountChange.bind(this);
    this.handleSubmit = this.handleSubmit.bind(this);
    this.handleSubmitClick = this.handleSubmitClick.bind(this);
    this.handleVoteWord = this.handleVoteWord.bind(this);
  }

  public componentDidMount() {
    //game has not been retrieved do not create a connection ever
    if (this.props.game.status) {
      //if we are supposed to be in the lobby and we don't have a connection make one
      if (this.props.game.status === "lobby" && !this.props.connection) {
        this.ensureConnectionExists();
      }
    }
  }

  private ensureConnectionExists() {
    this.props.createConnection();
  }

  handleWordChange(event: React.ChangeEvent<HTMLInputElement>) {
    this.setState({ hintWord: event.target.value });
  }

  handleCountChange(event: React.ChangeEvent<HTMLInputElement>) {
    this.setState({ wordCount: parseInt(event.target.value) });
  }

  handleSubmitClick() {
    this.props.giveHint({
      hintWord: this.state.hintWord,
      wordCount: this.state.wordCount
    } as GameStore.Hint);
  }

  handleVoteWord(word: string, isRevealed: boolean) {
    //ensure user is allowed to vote and not casting vote for revealed tile
    if (this.props.game.actions.canVote && !isRevealed) {
      this.props.voteWord({ word: word });
    }
  }

  handleSubmit(event: React.MouseEvent) {
    event.preventDefault();
  }

  public render() {
    if (!this.props.game.status || this.props.game.status !== "inProgress") {
      return <Redirect to="/game-home" />;
    }
    let currentTeam;
    let apposingTeam = this.props.localPlayer.team === "red" ? "blue" : "red";
    //probably a better way to do this?
    let localTeamsTilesRemaining =
      this.props.localPlayer.team === "red"
        ? this.props.game.redTilesRemaining
        : this.props.game.blueTilesRemaining;
    let apposingTeamTilesRemaining =
      this.props.localPlayer.team === "red"
        ? this.props.game.redTilesRemaining
        : this.props.game.blueTilesRemaining;
    let currentStatus: string = "";
    let hintWord;
    let wordCount;
    let guessesRemaining;
    if (this.props.game.currentTurn) {
      currentTeam = this.props.game.currentTurn.team;
      currentStatus = this.props.game.currentTurn.status;
      hintWord = this.props.game.currentTurn.hintWord;
      wordCount = this.props.game.currentTurn.wordCount;
      guessesRemaining = this.props.game.currentTurn.guessesRemaining;
    }

    return (
      <React.Fragment>
        <div>
          <div className="game-banner">
            <TeamTile
              team={this.props.localPlayer.team}
              tilesRemaining={localTeamsTilesRemaining}
              isTeamsTurn={currentTeam === this.props.localPlayer.team}
            />
            <TeamTile
              team={apposingTeam}
              tilesRemaining={apposingTeamTilesRemaining}
              isTeamsTurn={currentTeam !== this.props.localPlayer.team}
            />
          </div>
          <div style={{ display: "grid", gridTemplateColumns: "50% 50%" }}>
            <PlayerTracker
              team={this.props.localPlayer.team}
              playerName={this.props.localPlayer.name}
            />
            {/* not sure where to put status */}
            <h4 style={{ textAlign: "center" }}>{currentStatus}</h4>
          </div>
          <div style={{ textAlign: "center" }}>
            {/* this should only be seen by spy master when it is hint phase */}
            {this.props.game.actions.canGiveHint && (
              <div className="input-group mx-auto">
                <div className="input-group-prepend">
                  <span className="input-group-text" id="">
                    Enter Hint & Count
                  </span>
                </div>
                <input
                  type="text"
                  value={this.state.hintWord}
                  onChange={this.handleWordChange}
                  className="form-control"
                  placeholder="Enter hint here..."
                />
                <input
                  type="number"
                  value={this.state.wordCount}
                  onChange={this.handleCountChange}
                  className="form-control"
                  min="0"
                />
                <div className="input-group-append">
                  <button
                    className="btn btn-outline-secondary"
                    type="button"
                    onClick={this.handleSubmitClick}
                  >
                    Submit hint
                  </button>
                </div>
              </div>
            )}
            {this.props.game.actions.canApproveHint && (
              <div>
                <h3>Pending hint: "{hintWord}"</h3>
                <button
                  type="button"
                  className="btn btn-primary"
                  style={{ width: 300 }}
                  onClick={() => {
                    this.props.approveHint();
                  }}
                >
                  Approve
                </button>
                <button
                  type="button"
                  className="btn btn-danger"
                  style={{ width: 300, marginLeft: "20px" }}
                  onClick={() => {
                    this.props.refuseHint();
                  }}
                >
                  Refuse
                </button>
              </div>
            )}
            {currentStatus === "guessing" && (
              <div>
                <h1>
                  Hint: "{hintWord}" ({wordCount})
                </h1>
                <h5>
                  {guessesRemaining
                    ? guessesRemaining + " guesses remaining"
                    : "Unlimited guesses remaining"}
                </h5>
              </div>
            )}
          </div>
          <div className="game-board" style={{ marginTop: "10px" }}>
            {this.props.game.wordTiles &&
              this.props.game.wordTiles.map(tile => (
                <GameTile
                  wordTile={tile}
                  key={tile.word}
                  localPlayer={this.props.localPlayer}
                  handleVoteWord={this.handleVoteWord}
                  turnStatus={currentStatus}
                />
              ))}
          </div>
          {this.props.game.actions.canVote && (
            <div className="row">
              <button
                className="btn btn-info"
                type="button"
                onClick={() => this.props.voteEndTurn()}
                style={{ marginTop: "10px", marginLeft: "20px" }}
              >
                End Turn (
                {this.props.game.currentTurn
                  ? this.props.game.currentTurn.endTurnVotes.length
                  : 0}
                )
              </button>
            </div>
          )}
          {this.props.game.actions.canDelete && (
            <div className="row">
              <button
                className="btn btn-danger"
                type="button"
                onClick={() => this.props.deleteGame(this.props.game.code)}
                style={{ marginTop: "10px", marginLeft: "20px" }}
              >
                Delete Game
              </button>
            </div>
          )}
        </div>
        <div className="game-event-window">
          <h6>Game Log</h6>
          {this.props.events &&
            this.props.events.map((event, index) => (
              <div key={index}>
                <div style={{ margin: "2px" }}>
                  <span
                    style={{
                      color: "#AAA"
                    }}
                  >
                    {"[" + event.timestamp + "] "}
                  </span>
                  <span
                    style={{
                      color: getColor(event.team, true)
                    }}
                  >
                    {event.player ? event.player : "Team " + event.team}
                  </span>
                  <span>{" " + event.message}</span>
                </div>
              </div>
            ))}
        </div>
      </React.Fragment>
    );
  }
}
export default connect(
  (state: ApplicationState) => state.game, // Selects which state properties are merged into the component's props
  GameStore.actionCreators // Selects which action creators are merged into the component's props
)(Game as any);

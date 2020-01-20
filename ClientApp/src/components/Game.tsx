import * as React from "react";
import { connect } from "react-redux";
import * as GameStore from "../store/Game";
import { ApplicationState } from "../store";
import { Redirect } from "react-router";
import { red, blue, tan, black, grey } from "../constants/ColorConstants";
import "./Game.css";

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

type GameTileProps = {
  wordTile: GameStore.WordTile;
  localPlayer: GameStore.Player;
  handleVoteWord: (word: string) => void;
};
const GameTile = ({ wordTile, localPlayer, handleVoteWord }: GameTileProps) => (
  <div
    onClick={() => handleVoteWord(wordTile.word)}
    className="word-tile"
    style={{
      textAlign: "center",
      backgroundColor: getColor(
        wordTile.team,
        localPlayer.isSpyMaster || wordTile.isRevealed
      ),
      padding: "10px"
    }}
  >
    <h6
      style={{
        backgroundColor: "rgba(255, 255, 255, 0.75)",
        padding: "5px",
        borderRadius: "5px",
        boxShadow:
          "0 1px 3px rgba(255, 255, 255, 0.24), 0 1px 3px rgba(255, 255, 255, 0.36)"
      }}
    >
      {wordTile.word}
    </h6>
    <h6
      style={{
        backgroundColor: "rgba(255, 255, 255, 0.75)",
        padding: "5px",
        borderRadius: "5px",
        boxShadow:
          "0 1px 3px rgba(255, 255, 255, 0.24), 0 1px 3px rgba(255, 255, 255, 0.36)"
      }}
    >
      Votes: {wordTile.votes.length}
    </h6>
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

  handleVoteWord(word: string) {
    this.props.voteWord({ word: word });
  }

  handleSubmit(event: React.MouseEvent) {
    event.preventDefault();
  }

  public render() {
    if (!this.props.game.status || this.props.game.status !== "inProgress") {
      return <Redirect to="/game-home" />;
    }
    let currentTeam;
    let currentStatus;
    if (this.props.game.currentTurn) {
      currentTeam = this.props.game.currentTurn.team;
      currentStatus = this.props.game.currentTurn.status;
    }

    return (
      <React.Fragment>
        <div>
          <div style={{ textAlign: "center" }}>
            <h1
              style={{
                color: getColor(
                  currentTeam === undefined ? "black" : currentTeam,
                  true
                )
              }}
            >
              Current Team: {currentTeam}
            </h1>
            <h3>{currentStatus}</h3>
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
                <h3>
                  Pending hint:{" "}
                  {this.props.game.currentTurn
                    ? this.props.game.currentTurn.hintWord
                    : ""}
                </h3>
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
      </React.Fragment>
    );
  }
}
export default connect(
  (state: ApplicationState) => state.game, // Selects which state properties are merged into the component's props
  GameStore.actionCreators // Selects which action creators are merged into the component's props
)(Game as any);

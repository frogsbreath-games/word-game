import * as React from "react";
import { connect } from "react-redux";

const GameHome = () => (
  <div>
    <h1>Code Word Game</h1>
    <p>This is a new page</p>
  </div>
);

export default connect()(GameHome);

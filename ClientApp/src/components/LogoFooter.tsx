import * as React from "react";
import Logo from "../assets/FrogsBreath.png";

const LogoFooter = () => {
  return (
    <div>
      <div style={{ textAlign: "center", paddingTop: "50px" }}>
        <img src={Logo} width="150px" />
        <p>© 2019 FrogsBreath Games</p>
      </div>
    </div>
  );
};

export default LogoFooter;

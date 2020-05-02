import * as React from "react";
import Logo from "../assets/FrogsBreath.png";

const LogoFooter = () => {
  return (
    <div
      style={{
        textAlign: "center",
        position: "absolute",
        bottom: 0,
        width: "100%"
      }}
    >
      <img src={Logo} width="150px" alt="Frogsbreath Logo" />
      <p>© 2020 Frogsbreath Games</p>
    </div>
  );
};

export default LogoFooter;

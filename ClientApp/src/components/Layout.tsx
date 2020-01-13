import * as React from "react";
import { Container } from "reactstrap";
import NavMenu from "./NavMenu";
import LogoFooter from "./LogoFooter";

export default (props: { children?: React.ReactNode }) => (
  <React.Fragment>
    {/* this is to keep the footer in place */}
    <div style={{ position: "relative", minHeight: "100vh" }}>
      <NavMenu />
      <div style={{ paddingBottom: "16rem" }}>
        <Container>{props.children}</Container>
      </div>
      <LogoFooter />
    </div>
  </React.Fragment>
);

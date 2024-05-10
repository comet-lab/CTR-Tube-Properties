% Data analysis for the experiment performed on 3/19/2024
close all; clear; clc;

% Times
t = 1:8;
t_pre = 1:7;
t_post = 7:8;
labels = {"0", "5", "10", "15", "20", "25", "30", "3 Days Later"};

% Tube Diameters (mm)
OD = [3.3, 2.64, 1.98];

% Number of speciments per tube OD
n = 5;

% Curvature after 0, 5, 10, ... 30 straightening cycles (m^-1)
c0 = [9.45848	10.95806	9.76129	9.15193	7.542866667	10.8693	11.37361	10.49672	7.77692	9.40525	12.39821	10.92625	14.00654444	7.00422	7.8074];
c5 = [8.787	10.40711	8.89844	8.486528571	7.18502	9.93548	10.38473	9.59079	7.09875	8.67904	11.6375	9.81888	12.70035	6.50861	7.50718];
c10 = [8.50696	9.95122	8.64727	8.328677778	6.89981	9.61719	10.24893	9.26305	6.95686	8.50131	11.31664	9.17421	12.45091	6.335077778	7.35335];
c15 = [8.53778	9.69072	8.42527	7.989733333	6.83501	9.44898	10.17055	9.08107	6.92046	8.24777	10.97829	9.04114	12.24281	6.33532	7.3305];
c20 = [8.4441	9.61317	8.26902	8.035633333	6.767044444	9.020866667	9.94204	8.91572	6.81808	8.15424	10.87978333	8.99874	11.9847	6.27768	7.10168];
c25 = [8.31847	9.35727	8.23375	7.98728	6.67679	8.95734	9.97758	8.600025	6.7141	8.06843	10.761725	8.8421	11.98308	6.24847	7.161866667];
c30 = [8.28965	9.315525	8.08371	7.90944	6.70279	8.86028	9.84546	8.5453	6.70086	7.94204	10.6281	8.685795	12.05612	6.255833333	7.03403];
c_3dayslater = [9.40043	11.00674	9.51715	9.09926	7.55398	10.34116	11.38182	10.02012	7.79818	9.25157	11.28535556	10.34251	13.69417	6.9619	7.84042];

% Calculate the Bending Radius: at 0, 5, 10, ... 30 straightening cycles (mm)
r0 = 1./c0 * 1000;
r5 = 1./c5 * 1000;
r10 = 1./c10 * 1000;
r15 = 1./c15 * 1000;
r20 = 1./c20 * 1000;
r25 = 1./c25 * 1000;
r30 = 1./c30 * 1000;
r_3days = 1./c_3dayslater * 1000;

%% Formatting

titleSize = 24;
axisLabelSize = 20;
legendSize = 20;
tickSize = 16;
lineWidth = 2.5;

%%  Plot the loss of curvature over time
for jj = 1 : n
    figure('WindowState','fullscreen'); grid on; hold on;
    
    for ii = 1 : length(OD)
        radii_pre = [r0(jj+n*(ii-1)) r5(jj+n*(ii-1)) r10(jj+n*(ii-1)) r15(jj+n*(ii-1)) r20(jj+n*(ii-1)) r25(jj+n*(ii-1)) r30(jj+n*(ii-1))];
        radii_post = [r30(jj+n*(ii-1)) r_3days(jj+n*(ii-1))];
        pre = plot(t_pre(isfinite(radii_pre)), radii_pre(isfinite(radii_pre)),"-o","LineWidth",2.5);
        post = plot(t_post(isfinite(radii_post)), radii_post(isfinite(radii_post)),"--o","LineWidth",2.5);
        orig = plot([t(1) t(end)], [r0(jj+n*(ii-1)) r0(jj+n*(ii-1))],":","LineWidth",2.5);
        post.SeriesIndex = pre.SeriesIndex;
        orig.SeriesIndex = pre.SeriesIndex;
    end

    ax=gca;
    ax.FontSize = tickSize;
    
    xlim([1 t(end)]);
    set(gca,'xtick',t,'xticklabel',labels)

    title(["Bending Radius During Straightening Experiment for Tubes in Straightening Experiment Set #" num2str(jj)])
    
    xlabel("Number of Straightening Cycles")
    ylabel("Bending Radius [mm]")
    ylim([65 165]);

    legend({"Thickest (OD=3.3 mm)", "", "", "Thicker (OD=2.64 mm)", "", "", "Thinner (OD=1.98 mm)"}, "Location", "south");

    saveas(gcf,"./straightening/set"+num2str(jj)+".png");
end

%% Plot the loss of curvature over time / group by tube OD
tube_type = {"Thickest", "Thicker", "Thinnest"};

for jj = 1 : length(OD)
    figure('WindowState','fullscreen'); grid on; hold on;
    
    for ii = 1 : n
        radii_pre = [r0(ii+n*(jj-1)) r5(ii+n*(jj-1)) r10(ii+n*(jj-1)) r15(ii+n*(jj-1)) r20(ii+n*(jj-1)) r25(ii+n*(jj-1)) r30(ii+n*(jj-1))];
        radii_post = [r30(ii+n*(jj-1)) r_3days(ii+n*(jj-1))];
        pre = plot(t_pre(isfinite(radii_pre)), radii_pre(isfinite(radii_pre)),"-o","LineWidth",lineWidth);
        post = plot(t_post(isfinite(radii_post)), radii_post(isfinite(radii_post)),"--o","LineWidth",lineWidth);
        orig = plot([t(1) t(end)], [r0(ii+n*(jj-1)) r0(ii+n*(jj-1))],":","LineWidth",lineWidth);
        post.SeriesIndex = pre.SeriesIndex;
        orig.SeriesIndex = pre.SeriesIndex;
    end

    ax=gca;
    ax.FontSize = tickSize;

    title_string = ["Bending Radius During Straightening Experiment" "for Tubes with Outer Diameter " + num2str(OD(jj)) + " mm"];
    title(title_string, "FontSize",titleSize);
    disp(title_string)

    set(gca,'xtick',t,'xticklabel',labels)
    xlabel("Number of Straightening Cycles", "FontSize", axisLabelSize)
    ylabel("Bending Radius [mm]", "FontSize", axisLabelSize)
    ylim([65 165]);

    legend({"Specimen 1", "", "Specimen 1 Initial Bending Radius", "Specimen 2", "", "Specimen 2 Initial Bending Radius", "Specimen 3", "", "Specimen 3 Initial Bending Radius", "Specimen 4", "", "Specimen 4 Initial Bending Radius", "Specimen 5", "", "Specimen 5 Initial Bending Radius"}, "Location", "eastoutside", "FontSize", legendSize);

    saveas(gcf,"./straightening/"+"straightening_"+tube_type{jj}+".png");
    saveas(gcf,"./straightening/"+"straightening_"+tube_type{jj}+".svg");
end

%% Plot all  the tubes on one figure

figure('WindowState','maximized'); grid on; hold on;

tube_type = {"Thickest", "Thicker", "Thinnest"};

for jj = 1 : n
    for ii = 1 : length(OD)
        radii_pre = [r0(jj+n*(ii-1)) r5(jj+n*(ii-1)) r10(jj+n*(ii-1)) r15(jj+n*(ii-1)) r20(jj+n*(ii-1)) r25(jj+n*(ii-1)) r30(jj+n*(ii-1))];
        radii_post = [r30(jj+n*(ii-1)) r_3days(jj+n*(ii-1))];
        pre = plot(t_pre(isfinite(radii_pre)), radii_pre(isfinite(radii_pre)),"-o","LineWidth",2.5);
        post = plot(t_post(isfinite(radii_post)), radii_post(isfinite(radii_post)),"--o","LineWidth",2.5);
        post.SeriesIndex = ii;
        pre.SeriesIndex = ii;
    end
end

ax=gca;
ax.FontSize = tickSize;

xlim([1 t(end)]);
set(gca,'xtick',t,'xticklabel',labels)

title("Curvature Over Time for Tubes in Repeated Straightening Experiment")

xlabel("Number of Straightening Cycles")
ylabel("Bending Radius [mm]")
ylim([65 165]);

legend({"Thickest (OD=3.3 mm)", "", "Thicker (OD=2.64 mm)", "", "Thinner (OD=1.98 mm)"}, "Location", "south");

saveas(gcf,"./straightening/straightening_all"+".png");




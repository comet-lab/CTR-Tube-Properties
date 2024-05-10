% Data analysis for the experiment performed on 3/19/2024
close all; clear; clc;

%% Load in the data

% Times
t = 1:12;
t_pre = 1:2;
t_post = [2:7 12];
labels = {"Before Straightening", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10"};

% Tube Diameters (mm)
OD = [3.3, 2.64, 1.98];

% Number of speciments per tube OD
n = 5;

% Curvature at given times
cbefore = [12.31509	9.35317	10.77079	8.97755	9.02948	10.93576	9.84198	11.35067	9.77144	7.76711	10.94743	10.89157	9.79995	13.80976	6.81244];
c0 = [10.99898	8.19979	8.8425	7.47346	7.34607	9.73037	8.43095	9.25481	8.37001	6.67386	9.67951	9.474	8.28616	11.68277	5.187355556];
c1 = [11.79913	8.84982	9.79127	8.10242	8.10905	10.471	9.26085	9.92032	9.07589	7.01026	10.45108	10.32932	8.87705	12.84899	5.63338];
c2 = [11.90246	8.97262	10.0623	8.24921	8.24762	10.56746	9.48005	10.05565	9.25702	7.0776	10.54447	10.39176	8.94338	12.93974	5.8217];
c3 = [11.96266	9.1217	10.17534	8.29336	8.34552	10.628	9.5002	10.16504	9.30938	7.11074	10.66663	10.49045	9.03873	13.16328	5.870277778];
c4 = [12.11279	9.13063	10.26544	8.39123	8.40418	10.64723	9.57359	10.21155	9.44372	7.24567	10.78294	10.64165	9.14143	13.27489	5.88544];
c5 = [12.11887	9.12957	10.27113	8.45179	8.52718	10.68106667	9.628825	10.3199	9.42408	7.262511111	10.81308889	10.70586	9.21806	13.32345	6.16668];
c10 = [12.29036	9.19655	10.47074	8.58639	8.58677	10.7699	9.67647	10.45208	9.61727	7.35244	10.84498	10.74436	9.32265	13.38594	6.22791];

% Calculate the Bending Radius: Nominal and at t+0hr, t+0.5hr, and t+4hr (mm)
rbefore = 1./cbefore * 1000;
r0 = 1./c0 * 1000;
r1 = 1./c1 * 1000;
r2 = 1./c2 * 1000;
r3 = 1./c3 * 1000;
r4 = 1./c4 * 1000;
r5 = 1./c5 * 1000;
r10 = 1./c10 * 1000;

% Weibull parameters
batch1_b_r = [.3177 .2804 .5592];
batch1_k_f = [13.0368 11.0154 10.9074];
batch1_k_r = [-2.0372 -1.285 -1.2269];
batch1_n_r = [9.9999 1.6881 1.1357];

% Weibull data
t_weibull = 0:0.01:10;
weibull_3_3_1 = 1000 ./ (batch1_k_r(1)*exp(-(t_weibull/batch1_n_r(1)).^batch1_b_r(1))+batch1_k_f(1));
weibull_2_64_1 = 1000 ./ (batch1_k_r(2)*exp(-(t_weibull/batch1_n_r(2)).^batch1_b_r(2))+batch1_k_f(2));
weibull_1_98_1 = 1000 ./ (batch1_k_r(3)*exp(-(t_weibull/batch1_n_r(3)).^batch1_b_r(3))+batch1_k_f(3));
weibull_batch1 = [weibull_3_3_1; weibull_2_64_1; weibull_1_98_1];

%% Formatting

titleSize = 24;
axisLabelSize = 20;
legendSize = 20;
tickSize = 16;
lineWidth = 2.5;
markerSize = 25;

colors = [0	0.447	0.741;
0.85	0.325	0.098;
0.929	0.694	0.125;
0.494	0.184	0.556;
0.466	0.674	0.188;
0.301	0.745	0.933;
0.635	0.078	0.184];

%% Plot curvature loss by batch

% Plot the loss of curvature over time
for jj = 1 : n
    figure('WindowState','maximized'); grid on; hold on;
    
    for ii = 1 : length(OD)
        radii = [r0(jj+n*(ii-1)) r1(jj+n*(ii-1)) r2(jj+n*(ii-1)) r3(jj+n*(ii-1)) r4(jj+n*(ii-1)) r5(jj+n*(ii-1)) r10(jj+n*(ii-1))];
        post_straightening = plot(t_post(isfinite(radii)), radii(isfinite(radii)),"-o","LineWidth",lineWidth,"Color",colors(ii,:));
        % pre_straightening = plot(t_pre, [rbefore(jj+n*(ii-1)) r0(jj+n*(ii-1))], "--o", "LineWidth",lineWidth,"Color",colors(ii,:));
        orig = plot([t(1) t(end)], [rbefore(jj+n*(ii-1)) rbefore(jj+n*(ii-1))], ":", "LineWidth",lineWidth,"Color",colors(ii,:));
        % if jj==1
        %     weibull = plot(t_weibull+2, weibull_batch1(ii,:),"-.","LineWidth",lineWidth,"Color",colors(ii+3,:));
        %     weibull.SeriesIndex = post_straightening.SeriesIndex;
        % end
        % pre_straightening.SeriesIndex = post_straightening.SeriesIndex;
        orig.SeriesIndex = post_straightening.SeriesIndex;
    end

    ax=gca;
    ax.FontSize = tickSize;

    title(["Change in Curvature after Removal from Straight Steel Tube" "for Nylon-12 Tubes in Batch #"+num2str(jj)], "FontSize",titleSize);

    xlim([2 t(end)]);
    set(gca,'xtick',t,'xticklabel',labels)
    xlabel("Time Since Straightening [min]", "FontSize", axisLabelSize);
    ylabel("Bending Radius [mm]", "FontSize", axisLabelSize);
    ylim([70 195]);

    % if jj==1
    %     legend({"Thickest (OD=3.3 mm)", "", "", "Thickest (OD=3.3 mm) Weibull Curve", "Thicker (OD=2.64 mm)", "", "", "Thicker (OD=2.64 mm) Weibull Curve", "Thinner (OD=1.98 mm)", "", "", "Thinner (OD=1.98 mm) Weibull Curve"}, "Location", "northeast", "FontSize", legendSize);
    %     ylim([75 110]);
    % else
        legend({"Thickest (OD=3.3 mm)", "", "Thicker (OD=2.64 mm)", "", "Thinner (OD=1.98 mm)", ""}, "Location", "northeast", "FontSize", legendSize);
    % end

    % if jj~=1
        saveas(gcf,"./unloading_response/batch"+num2str(jj)+".png");
    % end
end

%% Plot the loss of curvature over time / group by tube OD
close all;

tube_type = {"Thickest", "Thicker", "Thinnest"};

for jj = 1 : length(OD)
    figure('WindowState','fullscreen'); grid on; hold on;
    
    for ii = 1 : n
        radii = [r0(ii+n*(jj-1)) r1(ii+n*(jj-1)) r2(ii+n*(jj-1)) r3(ii+n*(jj-1)) r4(ii+n*(jj-1)) r5(ii+n*(jj-1)) r10(ii+n*(jj-1))];
        post_straightening = plot(t_post(isfinite(radii)), radii(isfinite(radii)),"-o","LineWidth",lineWidth);
        % pre_straightening = plot(t_pre, [rbefore(ii+n*(jj-1)) r0(ii+n*(jj-1))],"--o", "LineWidth",lineWidth);
        orig = plot([t(1) t(end)], [rbefore(ii+n*(jj-1)) rbefore(ii+n*(jj-1))], ":", "LineWidth",lineWidth);
        % pre_straightening.SeriesIndex = post_straightening.SeriesIndex;
        orig.SeriesIndex = post_straightening.SeriesIndex;
    end

    ax=gca;
    ax.FontSize = tickSize;

    title_string = ["Change in Curvature after Removal from Straight Steel Tube" "for Nylon-12 Tubes with Outer Diameter " + num2str(OD(jj)) + " mm"];
    title(title_string, "FontSize",titleSize);

    xlim([2 t(end)]);
    set(gca,'xtick',t,'xticklabel',labels)
    xlabel("Time Since Straightening [min]", "FontSize", axisLabelSize)
    ylabel("Bending Radius [mm]", "FontSize", axisLabelSize)
    ylim([70 195]);

    legend({"Specimen 1", "Specimen 1 Initial Bending Radius", "Specimen 2", "Specimen 2 Initial Bending Radius","Specimen 3", "Specimen 3 Initial Bending Radius","Specimen 4", "Specimen 4 Initial Bending Radius","Specimen 5", "Specimen 5 Initial Bending Radius"}, "Location", "eastoutside", "FontSize", legendSize);

    saveas(gcf,"./unloading_response/"+"unloading_response"+tube_type{jj}+".png");
    saveas(gcf,"./unloading_response/"+"unloading_response"+tube_type{jj}+".svg");
end

% %% Plot curvature loss by batch (again)
% 
% % Plot the loss of curvature over time
% for jj = 1 : n
%     figure('WindowState','maximized'); grid on; hold on;
% 
%     for ii = 1 : length(OD)
%         radii = [r0(jj+n*(ii-1)) r1(jj+n*(ii-1)) r2(jj+n*(ii-1)) r3(jj+n*(ii-1)) r4(jj+n*(ii-1)) r5(jj+n*(ii-1)) r10(jj+n*(ii-1))];
%         post_straightening = plot(t_post(isfinite(radii)), radii(isfinite(radii)),"-o","LineWidth",lineWidth,"Color",colors(ii,:));
%         % pre_straightening = plot(t_pre, [rbefore(jj+n*(ii-1)) r0(jj+n*(ii-1))], "--o", "LineWidth",lineWidth,"Color",colors(ii,:));
%         orig = plot([t(1) t(end)], [rbefore(jj+n*(ii-1)) rbefore(jj+n*(ii-1))], ":", "LineWidth",lineWidth,"Color",colors(ii,:));
%         if jj==1
%             weibull = plot(t_weibull+2, weibull_batch1(ii,:),"-.","LineWidth",lineWidth,"Color",colors(ii+3,:));
%             weibull.SeriesIndex = post_straightening.SeriesIndex;
%         end
%         % pre_straightening.SeriesIndex = post_straightening.SeriesIndex;
%         orig.SeriesIndex = post_straightening.SeriesIndex;
%     end
% 
%     ax=gca;
%     ax.FontSize = tickSize;
% 
%     title(["Change in Curvature after Removal from Straight Steel Tube" "for Nylon-12 Tubes in Batch #"+num2str(jj)], "FontSize",titleSize);
% 
%     xlim([2 t(end)]);
%     set(gca,'xtick',t,'xticklabel',labels)
%     xlabel("Time Since Straightening [min]", "FontSize", axisLabelSize);
%     ylabel("Bending Radius [mm]", "FontSize", axisLabelSize);
%     ylim([70 115]);
% 
%     if jj==1
%         legend({"Thickest (OD=3.3 mm) Observed Radius", "Thickest (OD=3.3 mm) Original Radius", "Thickest (OD=3.3 mm) Weibull Curve", "Thicker (OD=2.64 mm) Observed Radius", "Thicker (OD=2.64 mm) Original Radius", "Thicker (OD=2.64 mm) Weibull Curve", "Thinner (OD=1.98 mm) Observed Radius", "Thinner (OD=1.98 mm) Original Radius", "Thinner (OD=1.98 mm) Weibull Curve"}, "Location", "northeast", "FontSize", legendSize);
%         ylim([80 115]);
%     else
%         legend({"Thickest (OD=3.3 mm) Observed Radius", "Thickest (OD=3.3 mm) Original Radius", "Thicker (OD=2.64 mm) Observed Radius", "Thicker (OD=2.64 mm) Original Radius", "Thinner (OD=1.98 mm) Observed Radius", "Thinner (OD=1.98 mm) Original Radius"}, "Location", "northeast", "FontSize", legendSize);
%     end
% 
%     % if jj~=1
%         saveas(gcf,"./unloading_response/batch"+num2str(jj)+"_for_defense_lastminute.png");
%     % end
% end

%% Assemble curve fitter data

all_c = [c0; c1; c2; c3; c4; c5; c10];
all_r = 1000./all_c;

t_data = [0:5 10];

tube_3_3_1 = all_c(:,1);
tube_3_3_2 = all_c(:,2);
tube_3_3_3 = all_c(:,3);
tube_3_3_4 = all_c(:,4);
tube_3_3_5 = all_c(:,5);
tube_2_64_1 = all_c(:,6);
tube_2_64_2 = all_c(:,7);
tube_2_64_3 = all_c(:,8);
tube_2_64_4 = all_c(:,9);
tube_2_64_5 = all_c(:,10);
tube_1_98_1 = all_c(:,11);
tube_1_98_2 = all_c(:,12);
tube_1_98_3 = all_c(:,13);
tube_1_98_4 = all_c(:,14);
tube_1_98_5 = all_c(:,15);

all_c_vector = [];
all_t_vector = [];
for ii=1:15
    all_t_vector = [all_t_vector; t_data'];
    all_c_vector = [all_c_vector; all_c(:,ii)];
end

all_r_vector = 1000./all_c_vector;

additive_offset_c = all_c;
for ii=1:15
    additive_offset_c(:,ii) = additive_offset_c(:,ii) - additive_offset_c(1,ii);
end

t_vector_5 = [t_data t_data t_data t_data t_data]';

additive_offset_c_3_3 = [additive_offset_c(:,1); 
                         additive_offset_c(:,2); 
                         additive_offset_c(:,3); 
                         additive_offset_c(:,4); 
                         additive_offset_c(:,5)];

additive_offset_c_2_64 = [additive_offset_c(:,6); 
                         additive_offset_c(:,7); 
                         additive_offset_c(:,8); 
                         additive_offset_c(:,9); 
                         additive_offset_c(:,10)];

additive_offset_c_1_98 = [additive_offset_c(:,11); 
                         additive_offset_c(:,12); 
                         additive_offset_c(:,13); 
                         additive_offset_c(:,14); 
                         additive_offset_c(:,15)];

additive_offset_c_vector = [];
for ii=1:15
    additive_offset_c_vector = [additive_offset_c_vector; additive_offset_c(:,ii)];
end

multiplicative_offset_c = all_c;
for ii=1:15
    multiplicative_offset_c(:,ii) = multiplicative_offset_c(:,ii) ./ multiplicative_offset_c(1,ii);
end

multiplicative_offset_c_3_3 = [multiplicative_offset_c(:,1); 
                         multiplicative_offset_c(:,2); 
                         multiplicative_offset_c(:,3); 
                         multiplicative_offset_c(:,4); 
                         multiplicative_offset_c(:,5)];

multiplicative_offset_c_2_64 = [multiplicative_offset_c(:,6); 
                         multiplicative_offset_c(:,7); 
                         multiplicative_offset_c(:,8); 
                         multiplicative_offset_c(:,9); 
                         multiplicative_offset_c(:,10)];

multiplicative_offset_c_1_98 = [multiplicative_offset_c(:,11); 
                         multiplicative_offset_c(:,12); 
                         multiplicative_offset_c(:,13); 
                         multiplicative_offset_c(:,14); 
                         multiplicative_offset_c(:,15)];

multiplicative_offset_c_vector = [];
for ii=1:15
    multiplicative_offset_c_vector = [multiplicative_offset_c_vector; multiplicative_offset_c(:,ii)];
end

close all;

weibull_multiplicative_b_r = 0.4583;
weibull_multiplicative_k_f = 1.1618;
weibull_multiplicative_k_r = -.1617;
weibull_multiplicative_n_r = 2.0821;
weibull_multiplicative = (weibull_multiplicative_k_r*exp(-(t_weibull/weibull_multiplicative_n_r).^weibull_multiplicative_b_r)+weibull_multiplicative_k_f);
% weibull_multiplicative = 100 * weibull_multiplicative; % Convert to percentage

low_conf_b_r = 0.1215;
low_conf_k_f = 1.0906;
low_conf_k_r = -0.2341;
low_conf_n_r = -1.5084;
low_conf = (low_conf_k_r*exp(-(t_weibull/low_conf_n_r).^low_conf_b_r)+low_conf_k_f);

high_conf_b_r = 0.7951;
high_conf_k_f = 1.2329;
high_conf_k_r = -0.0894;
high_conf_n_r = 5.6727;
high_conf = (high_conf_k_r*exp(-(t_weibull/high_conf_n_r).^high_conf_b_r)+high_conf_k_f);


figure("WindowState","maximized"); grid on; hold on;
plot(all_t_vector, multiplicative_offset_c_vector, ".", "MarkerSize",markerSize);
best_fit_curve = plot(t_weibull, weibull_multiplicative, "LineWidth",lineWidth);
low_conf_curve = plot(t_weibull, low_conf, "--", "LineWidth",lineWidth);
high_conf_curve = plot(t_weibull, high_conf, "--", "LineWidth",lineWidth);
low_conf_curve.SeriesIndex = best_fit_curve.SeriesIndex;
high_conf_curve.SeriesIndex = best_fit_curve.SeriesIndex;
ax=gca;
ax.FontSize = tickSize;
xlabel("Time Since Straightening [min]", "FontSize", axisLabelSize);
ylabel("Curvature Divided by Curvature at t=0", "FontSize", axisLabelSize);
title("Weibull Curve fit to Scaled Data from Unloading Response Experiment", "FontSize", titleSize);
legend(["Scaled Curvature Measurements", "Best-Fit Weibull Curve", "Weibull Curve Confidence Bounds"], "FontSize",legendSize, "Location","southeast");
